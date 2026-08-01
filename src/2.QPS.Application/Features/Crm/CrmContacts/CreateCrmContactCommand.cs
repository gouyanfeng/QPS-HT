using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class CreateCrmContactCommand : IRequest<bool>
{
    public Guid CustomerId { get; set; }

    public CrmContactCreateRequest Request { get; set; } = null!;
}

public class CreateCrmContactHandler : IRequestHandler<CreateCrmContactCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 创建客户联系人处理器。
    /// </summary>
    public CreateCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排新增联系人用例。
    /// </summary>
    public async Task<bool> Handle(CreateCrmContactCommand request, CancellationToken cancellationToken)
    {
        // 编排新增联系人用例：
        // 确认客户、创建联系人、同步主联系人摘要。
        var customer = await GetCustomer(request.CustomerId, cancellationToken);

        var contact = CreateContact(request, customer);

        await ApplyPrimaryContact(customer, contact, cancellationToken);

        _dbContext.CrmContacts.Add(contact);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取联系人所属客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(
                c => c.Id == customerId && !c.IsDeleted,
                cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        return customer;
    }

    /// <summary>
    /// 根据请求创建联系人实体。
    /// </summary>
    private static CrmContact CreateContact(CreateCrmContactCommand command, CrmHerbBase customer)
    {
        var shouldBePrimary = command.Request.IsPrimary ||
            (
                string.IsNullOrWhiteSpace(customer.PrimaryContactName) &&
                string.IsNullOrWhiteSpace(customer.PrimaryContactPhone));

        return CrmContact.Create(
            CustomerEntityType,
            command.CustomerId,
            command.Request.ContactName,
            command.Request.Phone,
            command.Request.PhoneType,
            command.Request.Wechat,
            command.Request.RoleName,
            shouldBePrimary,
            command.Request.Remark);
    }

    /// <summary>
    /// 联系人为主联系人时同步客户主联系人摘要。
    /// </summary>
    private async Task ApplyPrimaryContact(CrmHerbBase customer, CrmContact contact, CancellationToken cancellationToken)
    {
        if (!contact.IsPrimary)
        {
            return;
        }

        await UnmarkSiblingPrimaryContacts(customer.Id, contact.Id, cancellationToken);
        
        customer.UpdatePrimaryContact(contact.ContactName, contact.Phone);
    }

    /// <summary>
    /// 取消同一客户下其他主联系人标记。
    /// </summary>
    private async Task UnmarkSiblingPrimaryContacts(Guid customerId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == CustomerEntityType &&
                c.EntityId == customerId &&
                c.Id != contactId &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}

