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
    public Guid HerbBaseSubjectId { get; set; }

    public CrmContactCreateRequest Request { get; set; } = null!;
}

public class CreateCrmContactHandler : IRequestHandler<CreateCrmContactCommand, bool>
{
    private const string HerbBaseSubjectEntityType = CrmCodes.HerbBaseSubjectEntityType;

    private readonly IDbContext _dbContext;

    /// <summary>
    /// 创建基地主体联系人处理器。
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
        // 确认基地主体、创建联系人、同步主联系人摘要。
        var subject = await GetSubject(request.HerbBaseSubjectId, cancellationToken);

        await EnsurePhoneNotDuplicated(request.HerbBaseSubjectId, request.Request.Phone, cancellationToken);

        var contact = CreateContact(request, subject);

        await ApplyPrimaryContact(subject, contact, cancellationToken);

        _dbContext.CrmContacts.Add(contact);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        await CrmHerbBaseSubjectScoreService.RecalculateAsync(_dbContext, subject.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取联系人所属的药材基地主体。
    /// </summary>
    private async Task<CrmHerbBaseSubject> GetSubject(Guid herbBaseSubjectId, CancellationToken cancellationToken)
    {
        var subject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(
                subject => subject.Id == herbBaseSubjectId,
                cancellationToken);

        if (subject == null)
        {
            throw new BusinessException(404, "药材基地主体不存在");
        }

        return subject;
    }

    private async Task EnsurePhoneNotDuplicated(Guid herbBaseSubjectId, string phone, CancellationToken cancellationToken)
    {
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return;
        }

        var exists = await _dbContext.CrmContacts.AnyAsync(
            contact =>
                contact.EntityType == HerbBaseSubjectEntityType &&
                contact.EntityId == herbBaseSubjectId &&
                contact.Phone == normalizedPhone,
            cancellationToken);

        if (exists)
        {
            throw new BusinessException(400, "该主体下已存在相同联系电话");
        }
    }

    /// <summary>
    /// 根据请求创建联系人实体。
    /// </summary>
    private static CrmContact CreateContact(CreateCrmContactCommand command, CrmHerbBaseSubject subject)
    {
        var shouldBePrimary = command.Request.IsPrimary ||
            (
                string.IsNullOrWhiteSpace(subject.PrimaryContactName) &&
                string.IsNullOrWhiteSpace(subject.PrimaryContactPhone));

        return CrmContact.Create(
            HerbBaseSubjectEntityType,
            command.HerbBaseSubjectId,
            command.Request.ContactName,
            command.Request.Phone,
            command.Request.PhoneType,
            command.Request.Wechat,
            command.Request.RoleName,
            shouldBePrimary,
            command.Request.Remark);
    }

    /// <summary>
    /// 联系人为主联系人时同步主体主联系人摘要。
    /// </summary>
    private async Task ApplyPrimaryContact(CrmHerbBaseSubject subject, CrmContact contact, CancellationToken cancellationToken)
    {
        if (!contact.IsPrimary)
        {
            return;
        }

        await UnmarkSiblingPrimaryContacts(subject.Id, contact.Id, cancellationToken);
        
        subject.UpdatePrimaryContact(contact.ContactName, contact.Phone);
    }

    /// <summary>
    /// 取消同一主体下其他主联系人标记。
    /// </summary>
    private async Task UnmarkSiblingPrimaryContacts(Guid herbBaseSubjectId, Guid contactId, CancellationToken cancellationToken)
    {
        var siblings = await _dbContext.CrmContacts
            .Where(c =>
                c.EntityType == HerbBaseSubjectEntityType &&
                c.EntityId == herbBaseSubjectId &&
                c.Id != contactId &&
                c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.UnmarkPrimary();
        }
    }
}

