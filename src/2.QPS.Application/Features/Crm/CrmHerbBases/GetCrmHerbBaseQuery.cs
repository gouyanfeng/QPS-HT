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

    public GetCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmHerbBaseDto> Handle(GetCrmHerbBaseQuery request, CancellationToken cancellationToken)
    {
        var dto = await (
            from customer in _dbContext.CrmHerbBases
            join owner in _dbContext.SystemUsers on customer.OwnerUserId equals owner.Id into ownerGroup
            from owner in ownerGroup.DefaultIfEmpty()
            where customer.Id == request.Id && !customer.IsDeleted
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

        dto.MainProducts = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                !attribute.IsDeleted &&
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.EntityId == dto.Id &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode)
            .OrderBy(attribute => attribute.SortOrder)
            .ThenBy(attribute => attribute.CreatedAt)
            .Select(attribute => attribute.AttributeValue)
            .ToListAsync(cancellationToken);

        return dto;
    }
}



