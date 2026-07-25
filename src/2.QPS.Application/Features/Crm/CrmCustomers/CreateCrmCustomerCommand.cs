using MediatR;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmCustomers;

/// <summary>
/// 创建客户命令
/// </summary>
public class CreateCrmCustomerCommand : IRequest<CrmCustomerDto>
{
    public CrmCustomerCreateRequest Request { get; set; } = null!;
}

/// <summary>
/// 创建客户处理器
/// </summary>
public class CreateCrmCustomerHandler : IRequestHandler<CreateCrmCustomerCommand, CrmCustomerDto>
{
    private readonly IDbContext _dbContext;

    public CreateCrmCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmCustomerDto> Handle(CreateCrmCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = CrmCustomer.Create(
            request.Request.CustomerName,
            request.Request.CustomerType,
            request.Request.MainProduct,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Province,
            request.Request.City,
            request.Request.Area,
            request.Request.Address,
            request.Request.Lat,
            request.Request.Lng,
            request.Request.SourcePlatform,
            request.Request.SourceLeadId,
            request.Request.OwnerUserId,
            request.Request.Remark,
            request.Request.ParentCustomerId
        );

        _dbContext.CrmCustomers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CrmCustomerDto
        {
            Id = customer.Id,
            ParentCustomerId = customer.ParentCustomerId,
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
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}