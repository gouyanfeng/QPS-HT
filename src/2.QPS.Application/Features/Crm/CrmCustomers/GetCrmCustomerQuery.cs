using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmCustomers;

/// <summary>
/// 获取客户详情查询
/// </summary>
public class GetCrmCustomerQuery : IRequest<CrmCustomerDto>
{
    /// <summary>
    /// 客户ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 获取客户详情处理器
/// </summary>
public class GetCrmCustomerHandler : IRequestHandler<GetCrmCustomerQuery, CrmCustomerDto>
{
    private readonly IDbContext _dbContext;

    public GetCrmCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmCustomerDto> Handle(GetCrmCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmCustomers
            .Include(c => c.ParentCustomer)
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        return new CrmCustomerDto
        {
            Id = customer.Id,
            ParentCustomerId = customer.ParentCustomerId,
            ParentCustomerName = customer.ParentCustomer?.CustomerName,
            CustomerName = customer.CustomerName,
            CustomerType = customer.CustomerType,
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
            SourceLeadId = customer.SourceLeadId,
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
    }
}
