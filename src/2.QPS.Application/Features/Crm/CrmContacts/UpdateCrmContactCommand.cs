using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmContactUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmContactHandler : IRequestHandler<UpdateCrmContactCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 更新客户联系人处理器。
    /// </summary>
    public UpdateCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排更新联系人用例。
    /// </summary>
    public async Task<bool> Handle(UpdateCrmContactCommand request, CancellationToken cancellationToken)
    {
        // 编排更新联系人用例：
        // 确认联系人和客户、更新联系人、同步主联系人摘要。
        var contact = await GetContact(request.Id, cancellationToken);
        EnsureCanSetPrimary(request.Request, contact);
        var customer = await GetCustomer(contact, cancellationToken);
        var wasPrimary = contact.IsPrimary;

        UpdateContact(contact, request.Request);
        await ApplyPrimaryContactChange(customer, contact, wasPrimary, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取要更新的联系人。
    /// </summary>
    private async Task<CrmContact> GetContact(Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.CrmContacts
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);

        if (contact == null)
        {
            throw new BusinessException(404, "联系人不存在");
        }

        return contact;
    }

    /// <summary>
    /// 校验无效联系人不能设为主联系人。
    /// </summary>
    private static void EnsureCanSetPrimary(CrmContactUpdateRequest request, CrmContact contact)
    {
        if (request.IsPrimary && contact.Status == InvalidStatus)
        {
            throw new BusinessException(400, "无效联系人不能设为主联系人");
        }
    }

    /// <summary>
    /// 获取联系人所属客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(CrmContact contact, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(
                c =>
                    c.Id == contact.EntityId &&
                    contact.EntityType == CustomerEntityType &&
                    !c.IsDeleted,
                cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        return customer;
    }

    /// <summary>
    /// 更新联系人基础信息。
    /// </summary>
    private static void UpdateContact(CrmContact contact, CrmContactUpdateRequest request)
    {
        contact.Update(
            request.ContactName,
            request.Phone,
            request.PhoneType,
            request.Wechat,
            request.RoleName,
            request.IsPrimary,
            request.Remark);
    }

    /// <summary>
    /// 根据主联系人状态变更同步客户主联系人摘要。
    /// </summary>
    private async Task ApplyPrimaryContactChange(
        CrmHerbBase customer,
        CrmContact contact,
        bool wasPrimary,
        CancellationToken cancellationToken)
    {
        if (contact.IsPrimary)
        {
            await UnmarkSiblingPrimaryContacts(contact.EntityType, contact.EntityId, contact.Id, cancellationToken);
            customer.UpdatePrimaryContact(contact.ContactName, contact.Phone);
            return;
        }

        if (wasPrimary)
        {
            await PromoteOldestValidContactOrClear(customer, contact.Id, cancellationToken);
        }
    }

    /// <summary>
    /// 取消同一业务实体下其他主联系人标记。
    /// </summary>
    private async Task UnmarkSiblingPrimaryContacts(string entityType, Guid entityId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == entityType &&
                c.EntityId == entityId &&
                c.Id != contactId &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }

    /// <summary>
    /// 原主联系人取消后提升最早有效联系人或清空客户摘要。
    /// </summary>
    private async Task PromoteOldestValidContactOrClear(CrmHerbBase customer, Guid excludedContactId, CancellationToken cancellationToken)
    {
        var replacement = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == CustomerEntityType &&
                c.EntityId == customer.Id &&
                c.Id != excludedContactId &&
                c.Status != InvalidStatus)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (replacement == null)
        {
            customer.ClearPrimaryContact();
            return;
        }

        replacement.MarkPrimary();
        customer.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }
}


