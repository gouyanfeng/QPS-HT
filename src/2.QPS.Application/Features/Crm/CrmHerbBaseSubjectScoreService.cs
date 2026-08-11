using Microsoft.EntityFrameworkCore;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm;

public static class CrmHerbBaseSubjectScoreService
{
    public static async Task RecalculateAsync(IDbContext dbContext, Guid subjectId, CancellationToken cancellationToken)
    {
        var subject = await dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(item => item.Id == subjectId, cancellationToken);
        if (subject == null)
        {
            return;
        }

        var bases = await dbContext.CrmHerbBases
            .Where(item => item.HerbBaseSubjectId == subjectId && !item.IsDeleted)
            .Select(item => new
            {
                item.Id,
                item.Scale,
                item.Province,
                item.City,
                item.Area,
                item.Address,
                item.SourcePlatform,
                item.Remark
            })
            .ToListAsync(cancellationToken);

        var baseIds = bases.Select(item => item.Id).ToList();
        var mainProductCount = baseIds.Count == 0
            ? 0
            : await dbContext.CrmBusinessEntityAttributes
                .Where(item =>
                    !item.IsDeleted &&
                    item.EntityType == CrmCodes.HerbBaseEntityType &&
                    item.AttributeCode == CrmCodes.MainProductAttributeCode &&
                    baseIds.Contains(item.EntityId) &&
                    item.AttributeValue != string.Empty)
                .Select(item => item.AttributeValue)
                .Distinct()
                .CountAsync(cancellationToken);

        var contacts = await dbContext.CrmContacts
            .Where(item =>
                !item.IsDeleted &&
                item.EntityType == CrmCodes.HerbBaseSubjectEntityType &&
                item.EntityId == subjectId)
            .Select(item => new
            {
                item.Phone,
                item.Status
            })
            .ToListAsync(cancellationToken);
        var validContacts = contacts.Where(item => item.Status != "INVALID").ToList();

        var input = new CrmHerbBaseSubjectScoreInput
        {
            Status = subject.Status,
            Scale = subject.Scale ?? bases.Sum(item => item.Scale ?? 0),
            BaseCount = bases.Count,
            MainProductCount = mainProductCount,
            HasPrimaryContactName = !string.IsNullOrWhiteSpace(subject.PrimaryContactName),
            HasPrimaryContactPhone = !string.IsNullOrWhiteSpace(subject.PrimaryContactPhone),
            HasValidContact = validContacts.Count > 0,
            HasValidContactPhone = validContacts.Any(item => !string.IsNullOrWhiteSpace(item.Phone)),
            LastFollowAt = subject.LastFollowAt,
            LastFollowResult = subject.LastFollowResult ?? string.Empty,
            HasRegion = bases.Any(item =>
                !string.IsNullOrWhiteSpace(item.Province) ||
                !string.IsNullOrWhiteSpace(item.City) ||
                !string.IsNullOrWhiteSpace(item.Area)),
            HasAddress = bases.Any(item => !string.IsNullOrWhiteSpace(item.Address)),
            HasSource = bases.Any(item => !string.IsNullOrWhiteSpace(item.SourcePlatform)),
            HasRemark = !string.IsNullOrWhiteSpace(subject.Remark) ||
                bases.Any(item => !string.IsNullOrWhiteSpace(item.Remark))
        };

        var result = CrmHerbBaseSubjectScoreRule.Calculate(input);
        subject.UpdateScoreGrade(result.Score, result.Grade);
    }
}
