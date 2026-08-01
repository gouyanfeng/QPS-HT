using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class UpdateCrmContactStatusCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public CrmContactStatusRequest Request { get; set; } = null!;
}

public class UpdateCrmContactStatusHandler : IRequestHandler<UpdateCrmContactStatusCommand, bool>
{
    private const string CustomerEntityType = CrmCodes.HerbBaseEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 更新联系人状态处理器。
    /// </summary>
    public UpdateCrmContactStatusHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排更新联系人状态用例。
    /// </summary>
    public async Task<bool> Handle(UpdateCrmContactStatusCommand request, CancellationToken cancellationToken)
    {
        // 编排更新联系人状态用例：
        // 获取联系人、确认客户、更新状态、必要时重选主联系人。
        var contact = await GetContact(request.Id, cancellationToken);

        var customer = await GetCustomer(contact, cancellationToken);

        var wasPrimary = contact.IsPrimary;

        contact.MarkStatus(request.Request.Status, request.Request.Remark);

        await ApplyInvalidPrimaryContact(wasPrimary, contact, customer, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取要更新状态的联系人。
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
    /// 获取联系人所属客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(CrmContact contact, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(
                c => c.Id == contact.EntityId &&
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
    /// 主联系人失效时清空摘要并提升备用联系人。
    /// </summary>
    private async Task ApplyInvalidPrimaryContact(
        bool wasPrimary,
        CrmContact contact,
        CrmHerbBase customer,
        CancellationToken cancellationToken)
    {
        if (!wasPrimary || contact.Status != InvalidStatus)
        {
            return;
        }

        contact.UnmarkPrimary();
        customer.ClearPrimaryContact();

        await PromoteOldestValidContact(customer, contact.Id, cancellationToken);
    }

    /// <summary>
    /// 提升最早创建的有效联系人为主联系人。
    /// </summary>
    private async Task PromoteOldestValidContact(CrmHerbBase customer, Guid excludedContactId, CancellationToken cancellationToken)
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
            return;
        }

        replacement.MarkPrimary();
        customer.UpdatePrimaryContact(replacement.ContactName, replacement.Phone);
    }
}
