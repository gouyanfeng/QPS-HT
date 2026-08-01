using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 获取药材基地详情查询
/// </summary>
public class GetCrmHerbBaseQuery : IRequest<CrmHerbBaseDto>
{
    /// <summary>
    /// 客户ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 获取药材基地详情处理器
/// </summary>
public class GetCrmHerbBaseHandler : IRequestHandler<GetCrmHerbBaseQuery, CrmHerbBaseDto>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 获取药材基地详情处理器。
    /// </summary>
    public GetCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排查询药材基地详情用例。
    /// </summary>
    public async Task<CrmHerbBaseDto> Handle(GetCrmHerbBaseQuery request, CancellationToken cancellationToken)
    {
        // 编排查询药材基地详情用例：
        // 查询基础资料、校验存在、补齐主营品类。
        var dto = await GetCustomerDto(request.Id, cancellationToken);

        dto.MainProducts = await GetMainProducts(dto.Id, cancellationToken);

        return dto;
    }

    /// <summary>
    /// 查询药材基地详情 DTO。
    /// </summary>
    private async Task<CrmHerbBaseDto> GetCustomerDto(Guid customerId, CancellationToken cancellationToken)
    {
        var dto = await (
            from customer in _dbContext.CrmHerbBases
            join owner in _dbContext.SystemUsers on customer.OwnerUserId equals owner.Id into ownerGroup
            from owner in ownerGroup.DefaultIfEmpty()
            where customer.Id == customerId && !customer.IsDeleted
            select new CrmHerbBaseDto
            {
                Id = customer.Id,
                ParentId = customer.ParentId,
                BaseName = customer.BaseName,
                HerbBaseName = customer.BaseName,
                SubjectName = customer.SubjectName,
                Grade = customer.Grade,
                Score = customer.Score,
                Province = customer.Province,
                City = customer.City,
                Area = customer.Area,
                Address = customer.Address,
                Lat = customer.Lat,
                Lng = customer.Lng,
                SourcePlatform = customer.SourcePlatform,
                SourceId = customer.SourceId,
                Status = customer.Status,
                OwnerUserId = customer.OwnerUserId,
                OwnerUserName = owner == null ? null : owner.RealName != string.Empty ? owner.RealName : owner.Username,
                Remark = customer.Remark,
                PrimaryContactName = customer.PrimaryContactName,
                PrimaryContactPhone = customer.PrimaryContactPhone,
                LastFollowAt = customer.LastFollowAt,
                LastFollowResult = customer.LastFollowResult,
                NextFollowAt = customer.NextFollowAt,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        return dto;
    }

    /// <summary>
    /// 查询药材基地主营品类。
    /// </summary>
    private async Task<List<string>> GetMainProducts(Guid customerId, CancellationToken cancellationToken)
    {
        return await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.EntityId == customerId &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode)
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .Select(attribute => attribute.AttributeValue)
            .ToListAsync(cancellationToken);
    }
}
