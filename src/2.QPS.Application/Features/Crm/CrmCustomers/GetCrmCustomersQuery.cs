using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Application.Extensions;

namespace QPS.Application.Features.Crm.CrmCustomers;

/// <summary>
/// 获取客户列表查询
/// </summary>
public class GetCrmCustomersQuery : PaginationRequest, IRequest<PaginationResponse<CrmCustomerDto>>
{
    /// <summary>
    /// 客户名称
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public string? CustomerType { get; set; }

    /// <summary>
    /// 客户等级
    /// </summary>
    public string? Grade { get; set; }

    /// <summary>
    /// 客户状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 负责人ID
    /// </summary>
    public Guid? OwnerUserId { get; set; }
}

/// <summary>
/// 获取客户列表处理器
/// </summary>
public class GetCrmCustomersHandler : IRequestHandler<GetCrmCustomersQuery, PaginationResponse<CrmCustomerDto>>
{
    private readonly IDbContext _dbContext;

    public GetCrmCustomersHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<CrmCustomerDto>> Handle(GetCrmCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.CrmCustomers.AsQueryable();

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.CustomerName))
        {
            query = query.Where(c => c.CustomerName.Contains(request.CustomerName));
        }

        if (!string.IsNullOrEmpty(request.CustomerType))
        {
            query = query.Where(c => c.CustomerType == request.CustomerType);
        }

        if (!string.IsNullOrEmpty(request.Grade))
        {
            query = query.Where(c => c.Grade == request.Grade);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        if (request.OwnerUserId.HasValue)
        {
            query = query.Where(c => c.OwnerUserId == request.OwnerUserId);
        }

        // 转换为DTO
        var dtoQuery = query.Select(c => new CrmCustomerDto
        {
            Id = c.Id,
            ParentCustomerId = c.ParentCustomerId,
            CustomerName = c.CustomerName,
            CustomerType = c.CustomerType,
            MainProduct = c.MainProduct,
            Grade = c.Grade,
            Score = c.Score,
            Province = c.Province,
            City = c.City,
            Area = c.Area,
            Address = c.Address,
            Lat = c.Lat,
            Lng = c.Lng,
            SourcePlatform = c.SourcePlatform,
            SourceLeadId = c.SourceLeadId,
            Status = c.Status,
            OwnerUserId = c.OwnerUserId,
            Remark = c.Remark,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}