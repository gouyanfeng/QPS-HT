using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmContacts;

public class SetPrimaryCrmContactCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class SetPrimaryCrmContactHandler : IRequestHandler<SetPrimaryCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;
    private const string InvalidStatus = "INVALID";

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 设置主联系人处理器。
    /// </summary>
    public SetPrimaryCrmContactHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排设置主联系人用例。
    /// </summary>
    public async Task<bool> Handle(SetPrimaryCrmContactCommand request, CancellationToken cancellationToken)
    {
        // 编排设置主联系人用例：
        // 获取联系人、确认主体、取消其他主联系人、同步主体主联系人摘要。
        var contact = await GetContact(request.Id, cancellationToken);

        EnsureContactCanBePrimary(contact);

        var subject = await GetSubject(contact, cancellationToken);

        await UnmarkSiblingPrimaryContacts(contact, cancellationToken);

        contact.MarkPrimary();
        
        subject.UpdatePrimaryContact(contact.ContactName, contact.Phone);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await CrmHerbBaseSubjectScoreService.RecalculateAsync(_dbContext, subject.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取要设置为主联系人的联系人。
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
    /// 确认联系人可以设置为主联系人。
    /// </summary>
    private static void EnsureContactCanBePrimary(CrmContact contact)
    {
        if (contact.Status == InvalidStatus)
        {
            throw new BusinessException(400, "无效联系人不能设为主联系人");
        }
    }

    /// <summary>
    /// 获取联系人所属的药材基地主体。
    /// </summary>
    private async Task<CrmHerbBaseSubject> GetSubject(CrmContact contact, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(
                item => item.Id == contact.EntityId &&
                    contact.EntityType == HerbBaseSubjectEntityType,
                cancellationToken);

        if (subject == null)
        {
            throw new BusinessException(404, "药材基地主体不存在");
        }

        return subject;
    }

    /// <summary>
    /// 取消同一客户下其他主联系人标记。
    /// </summary>
    private async Task UnmarkSiblingPrimaryContacts(CrmContact contact, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == contact.EntityType &&
                c.EntityId == contact.EntityId &&
                c.Id != contact.Id &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}
