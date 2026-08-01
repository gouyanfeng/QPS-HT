using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 更新药材基地命令
/// </summary>
public class UpdateCrmHerbBaseCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public CrmHerbBaseUpdateRequest Request { get; set; } = null!;
}

/// <summary>
/// 更新药材基地处理器
/// </summary>
public class UpdateCrmHerbBaseHandler : IRequestHandler<UpdateCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;

    public UpdateCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var baseName = string.IsNullOrWhiteSpace(request.Request.BaseName)
            ? request.Request.HerbBaseName
            : request.Request.BaseName;

        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        // 更新基本信息
        customer.UpdateBasicInfo(
            baseName,
            request.Request.Grade,
            request.Request.Score,
            request.Request.Province,
            request.Request.City,
            request.Request.Area,
            request.Request.Address,
            request.Request.Lat,
            request.Request.Lng,
            request.Request.Remark,
            request.Request.SubjectName
        );

        // 更新上级客户
        if (customer.ParentId != request.Request.ParentId)
        {
            customer.SetParent(request.Request.ParentId);
        }

        // 更新负责人
        if (customer.OwnerUserId != request.Request.OwnerUserId)
        {
            customer.AssignOwner(request.Request.OwnerUserId);
        }

        if (customer.SourcePlatform != request.Request.SourcePlatform ||
            customer.SourceId != request.Request.SourceId)
        {
            customer.UpdateSource(
                request.Request.SourcePlatform,
                request.Request.SourceId);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.PrimaryContactName) ||
            !string.IsNullOrWhiteSpace(request.Request.PrimaryContactPhone))
        {
            var primaryContactName = string.IsNullOrWhiteSpace(request.Request.PrimaryContactName)
                ? customer.PrimaryContactName
                : request.Request.PrimaryContactName!;
            var primaryContactPhone = string.IsNullOrWhiteSpace(request.Request.PrimaryContactPhone)
                ? customer.PrimaryContactPhone
                : request.Request.PrimaryContactPhone!;

            customer.UpdatePrimaryContact(
                primaryContactName,
                primaryContactPhone);
        }

        // 更新状态
        if (!string.IsNullOrEmpty(request.Request.Status) && customer.Status != request.Request.Status)
        {
            customer.UpdateStatus(request.Request.Status, request.Request.Remark);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}



