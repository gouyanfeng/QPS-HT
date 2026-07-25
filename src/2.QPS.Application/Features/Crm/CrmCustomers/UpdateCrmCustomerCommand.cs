using MediatR;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmCustomers;

/// <summary>
/// 更新客户命令
/// </summary>
public class UpdateCrmCustomerCommand : IRequest<CrmCustomerDto>
{
    public Guid Id { get; set; }
    public CrmCustomerUpdateRequest Request { get; set; } = null!;
}

/// <summary>
/// 更新客户处理器
/// </summary>
public class UpdateCrmCustomerHandler : IRequestHandler<UpdateCrmCustomerCommand, CrmCustomerDto>
{
    private readonly IDbContext _dbContext;

    public UpdateCrmCustomerHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CrmCustomerDto> Handle(UpdateCrmCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmCustomers.FindAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "客户不存在");
        }

        // 更新基本信息
        customer.UpdateBasicInfo(
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
            request.Request.Remark
        );

        // 更新上级客户
        if (customer.ParentCustomerId != request.Request.ParentCustomerId)
        {
            customer.SetParent(request.Request.ParentCustomerId);
        }

        // 更新负责人
        if (customer.OwnerUserId != request.Request.OwnerUserId)
        {
            customer.AssignOwner(request.Request.OwnerUserId);
        }

        // 更新状态
        if (!string.IsNullOrEmpty(request.Request.Status) && customer.Status != request.Request.Status)
        {
            customer.UpdateStatus(request.Request.Status, request.Request.Remark);
        }

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