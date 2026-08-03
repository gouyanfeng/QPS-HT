using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Application.Extensions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 获取药材基地列表查询
/// </summary>
public class GetCrmHerbBasesQuery : PaginationRequest, IRequest<PaginationResponse<CrmHerbBaseDto>>
{
    /// <summary>
    /// 客户名称
    /// </summary>
    public string? HerbBaseName { get; set; }

    public string? BaseName { get; set; }

    public string? Keyword { get; set; }

    /// <summary>
    /// 药材基地等级
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>
    /// 药材基地状态
    /// </summary>
    public string? Status { get; set; }

    public string? SourcePlatform { get; set; }

    /// <summary>
    /// 负责人ID
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    public List<string>? MainProducts { get; set; }

    public string? Province { get; set; }

    public string? City { get; set; }

    public DateTime? NextFollowFrom { get; set; }

    public DateTime? NextFollowTo { get; set; }

    public bool? OnlyOverdue { get; set; }

    public bool? OnlyNoNextFollow { get; set; }
}

/// <summary>
/// 获取药材基地列表处理器
/// </summary>
public class GetCrmHerbBasesHandler : IRequestHandler<GetCrmHerbBasesQuery, PaginationResponse<CrmHerbBaseDto>>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 获取药材基地列表处理器。
    /// </summary>
    public GetCrmHerbBasesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排查询药材基地列表用例。
    /// </summary>
    public async Task<PaginationResponse<CrmHerbBaseDto>> Handle(GetCrmHerbBasesQuery request, CancellationToken cancellationToken)
    {
        // 编排查询药材基地列表用例：
        // 构建过滤条件、分页映射 DTO、补齐主营品类。
        var query = _dbContext.CrmHerbBases
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // 应用查询条件
        var baseNameFilter = string.IsNullOrWhiteSpace(request.BaseName)
            ? request.HerbBaseName
            : request.BaseName;

        if (!string.IsNullOrEmpty(baseNameFilter))
        {
            query = query.Where(c => c.BaseName.Contains(baseNameFilter));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword!;
            query = query.Where(c =>
                c.BaseName.Contains(keyword) ||
                c.SubjectName.Contains(keyword) ||
                c.PrimaryContactName.Contains(keyword) ||
                c.PrimaryContactPhone.Contains(keyword));
        }

        var mainProducts = NormalizeMainProducts(request.MainProducts);
        if (mainProducts.Count == 1)
        {
            var mainProduct = mainProducts[0];
            query = query.Where(c =>
                _dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                    !attribute.IsDeleted &&
                    attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                    attribute.EntityId == c.Id &&
                    attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                    attribute.AttributeValue == mainProduct));
        }
        else if (mainProducts.Count > 1)
        {
            query = query.Where(c => _dbContext.CrmBusinessEntityAttributes.Any(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.EntityId == c.Id &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                mainProducts.Contains(attribute.AttributeValue)));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            var province = request.Province!;
            query = query.Where(c => c.Province == province);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City!;
            query = query.Where(c => c.City == city);
        }

        if (!string.IsNullOrEmpty(request.Grade))
        {
            query = query.Where(c => c.Grade == request.Grade);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.SourcePlatform))
        {
            query = query.Where(c => c.SourcePlatform == request.SourcePlatform);
        }

        if (request.OwnerUserId.HasValue)
        {
            query = query.Where(c => c.OwnerUserId == request.OwnerUserId);
        }

        if (request.NextFollowFrom.HasValue)
        {
            query = query.Where(c => c.NextFollowAt >= request.NextFollowFrom.Value);
        }

        if (request.NextFollowTo.HasValue)
        {
            query = query.Where(c => c.NextFollowAt <= request.NextFollowTo.Value);
        }

        if (request.OnlyOverdue == true)
        {
            var now = DateTime.Now;
            query = query.Where(c => c.NextFollowAt.HasValue && c.NextFollowAt.Value < now);
        }

        if (request.OnlyNoNextFollow == true)
        {
            query = query.Where(c => c.NextFollowAt == null);
        }

        // 转换为DTO
        var dtoQuery =
            from c in query
            join owner in _dbContext.SystemUsers on c.OwnerUserId equals owner.Id into ownerGroup
            from owner in ownerGroup.DefaultIfEmpty()
            select new CrmHerbBaseDto
            {
                Id = c.Id,
                ParentId = c.ParentId,
                BaseName = c.BaseName,
                HerbBaseName = c.BaseName,
                SubjectName = c.SubjectName,
                Grade = c.Grade,
                Score = c.Score,
                Scale = c.Scale,
                Province = c.Province,
                City = c.City,
                Area = c.Area,
                Address = c.Address,
                Lat = c.Lat,
                Lng = c.Lng,
                SourcePlatform = c.SourcePlatform,
                SourceId = c.SourceId,
                Status = c.Status,
                OwnerUserId = c.OwnerUserId,
                OwnerUserName = owner == null ? null : owner.RealName != string.Empty ? owner.RealName : owner.Username,
                Remark = c.Remark,
                PrimaryContactName = c.PrimaryContactName,
                PrimaryContactPhone = c.PrimaryContactPhone,
                LastFollowAt = c.LastFollowAt,
                LastFollowResult = c.LastFollowResult,
                NextFollowAt = c.NextFollowAt,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };

        // 执行分页查询
        var response = await dtoQuery.ToPaginationResponseAsync(request);
        await FillMainProductsAsync(response.List, cancellationToken);

        return response;
    }

    /// <summary>
    /// 规范化主营品类筛选值。
    /// </summary>
    private static List<string> NormalizeMainProducts(IEnumerable<string>? mainProducts)
    {
        return (mainProducts ?? Enumerable.Empty<string>())
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// 补齐列表中的主营品类。
    /// </summary>
    private async Task FillMainProductsAsync(List<CrmHerbBaseDto> herbBases, CancellationToken cancellationToken)
    {
        var herbBaseIds = herbBases.Select(herbBase => herbBase.Id).ToList();
        if (herbBaseIds.Count == 0)
        {
            return;
        }

        var attributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode &&
                herbBaseIds.Contains(attribute.EntityId))
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .ToListAsync(cancellationToken);

        var lookup = attributes
            .GroupBy(attribute => attribute.EntityId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(attribute => attribute.AttributeValue).ToList());

        foreach (var herbBase in herbBases)
        {
            herbBase.MainProducts = lookup.TryGetValue(herbBase.Id, out var mainProducts)
                ? mainProducts
                : new List<string>();
        }
    }
}



