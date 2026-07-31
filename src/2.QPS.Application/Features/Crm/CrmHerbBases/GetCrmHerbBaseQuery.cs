using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
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
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        var dto = new CrmHerbBaseDto
        {
            Id = customer.Id,
            ParentId = customer.ParentId,
            BaseName = customer.BaseName,
            HerbBaseName = customer.BaseName,
            SubjectName = customer.SubjectName,
            MainProduct = customer.MainProduct,
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
            Remark = customer.Remark,
            PrimaryContactName = customer.PrimaryContactName,
            PrimaryContactPhone = customer.PrimaryContactPhone,
            LastFollowAt = customer.LastFollowAt,
            LastFollowResult = customer.LastFollowResult,
            NextFollowAt = customer.NextFollowAt,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

        await CrmHerbBaseMainProducts.FillAsync(_dbContext, new List<CrmHerbBaseDto> { dto }, cancellationToken);
        await CrmHerbBaseOwners.FillAsync(_dbContext, new List<CrmHerbBaseDto> { dto }, cancellationToken);

        return dto;
    }
}



