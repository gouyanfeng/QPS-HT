using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public class GetCrmHerbBaseSubjectQuery : IRequest<CrmHerbBaseSubjectDetailDto>
{
    public Guid Id { get; set; }
}

public class GetCrmHerbBaseSubjectHandler : IRequestHandler<GetCrmHerbBaseSubjectQuery, CrmHerbBaseSubjectDetailDto>
{
    private readonly IDbContext _dbContext;

    public GetCrmHerbBaseSubjectHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmHerbBaseSubjectDetailDto> Handle(
        GetCrmHerbBaseSubjectQuery request,
        CancellationToken cancellationToken)
    {
        var subject = await GetSubjectAsync(request.Id, cancellationToken);
        subject.HerbBases = await GetHerbBasesAsync(subject.Id, cancellationToken);
        await FillMainProductsAsync(subject.HerbBases, cancellationToken);
        FillBaseSummary(subject);
        subject.Contacts = await GetContactsAsync(subject.Id, cancellationToken);
        subject.FollowRecords = await GetFollowRecordsAsync(subject.Id, cancellationToken);
        subject.TransferRecords = await GetTransferRecordsAsync(subject.Id, cancellationToken);
        return subject;
    }

    private async Task<CrmHerbBaseSubjectDetailDto> GetSubjectAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        var subject = await (
            from item in _dbContext.CrmHerbBaseSubjects
            join owner in _dbContext.SystemUsers on item.OwnerUserId equals owner.Id into ownerGroup
            from owner in ownerGroup.DefaultIfEmpty()
            where item.Id == subjectId
            select new CrmHerbBaseSubjectDetailDto
            {
                Id = item.Id,
                SubjectName = item.SubjectName,
                SubjectType = item.SubjectType,
                OwnerUserId = item.OwnerUserId,
                OwnerUserName = owner == null ? null : owner.RealName != string.Empty ? owner.RealName : owner.Username,
                Status = item.Status,
                Grade = item.Grade,
                Score = item.Score,
                PrimaryContactName = item.PrimaryContactName,
                PrimaryContactPhone = item.PrimaryContactPhone,
                LastFollowAt = item.LastFollowAt,
                LastFollowResult = item.LastFollowResult,
                NextFollowAt = item.NextFollowAt,
                TotalScale = item.Scale ?? 0,
                Remark = item.Remark,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            }).FirstOrDefaultAsync(cancellationToken);

        return subject ?? throw new BusinessException(404, "药材基地主体不存在");
    }

    private async Task<List<CrmHerbBaseDto>> GetHerbBasesAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmHerbBases
            .Where(herbBase => herbBase.HerbBaseSubjectId == subjectId)
            .OrderByDescending(herbBase => herbBase.UpdatedAt)
            .Select(herbBase => new CrmHerbBaseDto
            {
                Id = herbBase.Id,
                HerbBaseSubjectId = herbBase.HerbBaseSubjectId,
                BaseName = herbBase.BaseName,
                SubjectName = herbBase.SubjectName,
                Grade = herbBase.Grade,
                Score = herbBase.Score,
                Scale = herbBase.Scale,
                Province = herbBase.Province,
                City = herbBase.City,
                Area = herbBase.Area,
                Address = herbBase.Address,
                Lat = herbBase.Lat,
                Lng = herbBase.Lng,
                SourcePlatform = herbBase.SourcePlatform,
                SourceId = herbBase.SourceId,
                Remark = herbBase.Remark,
                CreatedAt = herbBase.CreatedAt,
                UpdatedAt = herbBase.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task FillMainProductsAsync(List<CrmHerbBaseDto> herbBases, CancellationToken cancellationToken)
    {
        var herbBaseIds = herbBases.Select(herbBase => herbBase.Id).ToList();
        if (herbBaseIds.Count == 0)
            return;

        var attributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                herbBaseIds.Contains(attribute.EntityId))
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .Select(attribute => new { attribute.EntityId, attribute.AttributeValue })
            .ToListAsync(cancellationToken);
        var productsByBase = attributes
            .GroupBy(attribute => attribute.EntityId)
            .ToDictionary(group => group.Key, group => group.Select(attribute => attribute.AttributeValue).ToList());

        foreach (var herbBase in herbBases)
            herbBase.MainProducts = productsByBase.GetValueOrDefault(herbBase.Id, []);
    }

    private static void FillBaseSummary(CrmHerbBaseSubjectDetailDto subject)
    {
        subject.BaseCount = subject.HerbBases.Count;
        subject.MainProducts = subject.HerbBases.SelectMany(herbBase => herbBase.MainProducts).Distinct().ToList();
        subject.Regions = subject.HerbBases
            .Select(herbBase => string.Join(' ', new[] { herbBase.Province, herbBase.City, herbBase.Area }.Where(value => !string.IsNullOrWhiteSpace(value))))
            .Where(region => !string.IsNullOrWhiteSpace(region))
            .Distinct()
            .ToList();
    }

    private async Task<List<CrmContactDto>> GetContactsAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmContacts
            .Where(contact => contact.EntityType == CrmCodes.HerbBaseSubjectEntityType && contact.EntityId == subjectId)
            .OrderByDescending(contact => contact.IsPrimary)
            .ThenBy(contact => contact.CreatedAt)
            .Select(contact => new CrmContactDto
            {
                Id = contact.Id,
                EntityType = contact.EntityType,
                EntityId = contact.EntityId,
                ContactName = contact.ContactName,
                Phone = contact.Phone,
                PhoneType = contact.PhoneType,
                Wechat = contact.Wechat,
                RoleName = contact.RoleName,
                IsPrimary = contact.IsPrimary,
                Status = contact.Status,
                Remark = contact.Remark,
                CreatedAt = contact.CreatedAt,
                UpdatedAt = contact.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CrmFollowRecordDto>> GetFollowRecordsAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmFollowRecords
            .Include(record => record.Contact)
            .Where(record => record.HerbBaseSubjectId == subjectId)
            .OrderByDescending(record => record.CreatedAt)
            .Select(record => new CrmFollowRecordDto
            {
                Id = record.Id,
                HerbBaseSubjectId = record.HerbBaseSubjectId,
                HerbBaseId = record.HerbBaseId,
                ContactId = record.ContactId,
                ContactName = record.Contact == null ? null : record.Contact.ContactName,
                FollowType = record.FollowType,
                FollowResult = record.FollowResult,
                IntentLevel = record.IntentLevel,
                Content = record.Content,
                NextFollowAt = record.NextFollowAt,
                OperatorUserId = record.OperatorUserId,
                CreatedAt = record.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CrmTransferRecordDto>> GetTransferRecordsAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        var records = await _dbContext.CrmTransferRecords
            .AsNoTracking()
            .Where(record =>
                record.EntityType == CrmCodes.HerbBaseSubjectEntityType &&
                record.EntityId == subjectId)
            .OrderByDescending(record => record.CreatedAt)
            .Select(record => new CrmTransferRecordDto
            {
                Id = record.Id,
                EntityType = record.EntityType,
                EntityId = record.EntityId,
                FromOwnerUserId = record.FromOwnerUserId,
                ToOwnerUserId = record.ToOwnerUserId,
                OperatorUserId = record.OperatorUserId,
                Remark = record.Remark,
                CreatedAt = record.CreatedAt
            })
            .ToListAsync(cancellationToken);

        await FillTransferUserNamesAsync(records, cancellationToken);
        return records;
    }

    private async Task FillTransferUserNamesAsync(List<CrmTransferRecordDto> records, CancellationToken cancellationToken)
    {
        var userIds = records
            .SelectMany(record => new[] { record.FromOwnerUserId, record.ToOwnerUserId, record.OperatorUserId })
            .Where(userId => userId.HasValue)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToList();
        if (userIds.Count == 0)
            return;

        var userNameLookup = await _dbContext.SystemUsers
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => string.IsNullOrWhiteSpace(user.RealName) ? user.Username : user.RealName,
                cancellationToken);

        foreach (var record in records)
        {
            record.FromOwnerUserName = GetUserName(userNameLookup, record.FromOwnerUserId);
            record.ToOwnerUserName = GetUserName(userNameLookup, record.ToOwnerUserId);
            record.OperatorUserName = GetUserName(userNameLookup, record.OperatorUserId);
        }
    }

    private static string GetUserName(Dictionary<Guid, string> userNameLookup, Guid? userId)
    {
        return userId.HasValue && userNameLookup.TryGetValue(userId.Value, out var userName)
            ? userName
            : string.Empty;
    }
}
