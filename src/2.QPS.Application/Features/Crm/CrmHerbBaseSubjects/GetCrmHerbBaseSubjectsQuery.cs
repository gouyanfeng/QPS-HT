using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.Crm.CrmHerbBaseSubjects;

public class GetCrmHerbBaseSubjectsQuery : PaginationRequest, IRequest<PaginationResponse<CrmHerbBaseSubjectDto>>
{
    public string? Keyword { get; set; }
    public string? Grade { get; set; }
    public string? Status { get; set; }
    public Guid? OwnerUserId { get; set; }
    public List<string>? MainProducts { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? SourcePlatform { get; set; }
    public DateTime? NextFollowFrom { get; set; }
    public DateTime? NextFollowTo { get; set; }
    public bool? OnlyOverdue { get; set; }
    public bool? OnlyNoNextFollow { get; set; }
}

public class GetCrmHerbBaseSubjectsHandler : IRequestHandler<GetCrmHerbBaseSubjectsQuery, PaginationResponse<CrmHerbBaseSubjectDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmHerbBaseSubjectsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CrmHerbBaseSubjectDto>> Handle(
        GetCrmHerbBaseSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilters(_dbContext.CrmHerbBaseSubjects.AsQueryable(), request);
        var dtoQuery =
            from subject in query
            join owner in _dbContext.SystemUsers on subject.OwnerUserId equals owner.Id into ownerGroup
            from owner in ownerGroup.DefaultIfEmpty()
            select new CrmHerbBaseSubjectDto
            {
                Id = subject.Id,
                SubjectName = subject.SubjectName,
                SubjectType = subject.SubjectType,
                OwnerUserId = subject.OwnerUserId,
                OwnerUserName = owner == null ? null : owner.RealName != string.Empty ? owner.RealName : owner.Username,
                Status = subject.Status,
                Grade = subject.Grade,
                Score = subject.Score,
                PrimaryContactName = subject.PrimaryContactName,
                PrimaryContactPhone = subject.PrimaryContactPhone,
                LastFollowAt = subject.LastFollowAt,
                LastFollowResult = subject.LastFollowResult,
                NextFollowAt = subject.NextFollowAt,
                Remark = subject.Remark,
                CreatedAt = subject.CreatedAt,
                UpdatedAt = subject.UpdatedAt
            };

        var response = await dtoQuery.ToPaginationResponseAsync(request);
        await FillBaseSummariesAsync(response.List, cancellationToken);
        return response;
    }

    private IQueryable<QPS.Domain.Entities.Crm.CrmHerbBaseSubject> ApplyFilters(
        IQueryable<QPS.Domain.Entities.Crm.CrmHerbBaseSubject> query,
        GetCrmHerbBaseSubjectsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(subject =>
                (subject.SubjectName ?? string.Empty).Contains(keyword) ||
                (subject.PrimaryContactName ?? string.Empty).Contains(keyword) ||
                (subject.PrimaryContactPhone ?? string.Empty).Contains(keyword) ||
                subject.HerbBases.Any(herbBase =>
                    herbBase.BaseName.Contains(keyword) ||
                    herbBase.Province.Contains(keyword) ||
                    herbBase.City.Contains(keyword) ||
                    herbBase.Area.Contains(keyword) ||
                    herbBase.Address.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(request.Grade))
            query = query.Where(subject => subject.Grade == request.Grade);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(subject => subject.Status == request.Status);

        if (request.OwnerUserId.HasValue)
            query = query.Where(subject => subject.OwnerUserId == request.OwnerUserId);

        if (!string.IsNullOrWhiteSpace(request.Province))
            query = query.Where(subject => subject.HerbBases.Any(herbBase => herbBase.Province == request.Province));

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(subject => subject.HerbBases.Any(herbBase => herbBase.City == request.City));

        if (!string.IsNullOrWhiteSpace(request.SourcePlatform))
            query = query.Where(subject => subject.HerbBases.Any(herbBase => herbBase.SourcePlatform == request.SourcePlatform));

        if (request.NextFollowFrom.HasValue)
            query = query.Where(subject => subject.NextFollowAt >= request.NextFollowFrom);

        if (request.NextFollowTo.HasValue)
            query = query.Where(subject => subject.NextFollowAt <= request.NextFollowTo);

        if (request.OnlyOverdue == true)
            query = query.Where(subject => subject.NextFollowAt.HasValue && subject.NextFollowAt < DateTime.Now);

        if (request.OnlyNoNextFollow == true)
            query = query.Where(subject => !subject.NextFollowAt.HasValue);

        var mainProducts = NormalizeMainProducts(request.MainProducts);
        if (mainProducts.Count > 0)
        {
            query = query.Where(subject => subject.HerbBases.Any(herbBase =>
                _dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                    attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                    attribute.EntityId == herbBase.Id &&
                    attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                    mainProducts.Contains(attribute.AttributeValue))));
        }

        return query;
    }

    private async Task FillBaseSummariesAsync(
        List<CrmHerbBaseSubjectDto> subjects,
        CancellationToken cancellationToken)
    {
        if (subjects.Count == 0)
            return;

        var subjectIds = subjects.Select(subject => subject.Id).ToList();
        var bases = await _dbContext.CrmHerbBases
            .Where(herbBase => herbBase.HerbBaseSubjectId.HasValue && subjectIds.Contains(herbBase.HerbBaseSubjectId.Value))
            .Select(herbBase => new
            {
                herbBase.Id,
                SubjectId = herbBase.HerbBaseSubjectId!.Value,
                herbBase.Scale,
                herbBase.Province,
                herbBase.City,
                herbBase.Area
            })
            .ToListAsync(cancellationToken);

        var baseIds = bases.Select(herbBase => herbBase.Id).ToList();
        var attributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                baseIds.Contains(attribute.EntityId))
            .Select(attribute => new { attribute.EntityId, attribute.AttributeValue })
            .ToListAsync(cancellationToken);
        var baseProducts = attributes
            .GroupBy(attribute => attribute.EntityId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.AttributeValue));

        foreach (var subject in subjects)
        {
            var subjectBases = bases.Where(herbBase => herbBase.SubjectId == subject.Id).ToList();
            subject.BaseCount = subjectBases.Count;
            subject.TotalScale = subjectBases.Sum(herbBase => herbBase.Scale ?? 0);
            subject.Regions = subjectBases
                .Select(herbBase => string.Join(' ', new[] { herbBase.Province, herbBase.City, herbBase.Area }.Where(value => !string.IsNullOrWhiteSpace(value))))
                .Where(region => !string.IsNullOrWhiteSpace(region))
                .Distinct()
                .ToList();
            subject.MainProducts = subjectBases
                .SelectMany(herbBase => baseProducts.TryGetValue(herbBase.Id, out var products)
                    ? products
                    : Enumerable.Empty<string>())
                .Distinct()
                .ToList();
        }
    }

    private static List<string> NormalizeMainProducts(IEnumerable<string>? mainProducts)
    {
        return (mainProducts ?? Enumerable.Empty<string>())
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
