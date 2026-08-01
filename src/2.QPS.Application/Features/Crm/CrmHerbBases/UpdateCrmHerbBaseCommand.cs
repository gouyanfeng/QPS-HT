using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
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

    /// <summary>
    /// 更新药材基地处理器。
    /// </summary>
    public UpdateCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排更新药材基地用例。
    /// </summary>
    public async Task<bool> Handle(UpdateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        // 编排更新药材基地用例：
        // 获取客户、更新基本信息、同步关联字段、保存。
        var customer = await GetCustomer(request.Id, cancellationToken);

        UpdateBasicInfo(customer, request.Request);

        ApplyParent(customer, request.Request.ParentId);

        ApplyOwner(customer, request.Request.OwnerUserId);

        ApplySource(customer, request.Request);

        ApplyPrimaryContact(customer, request.Request);

        ApplyStatus(customer, request.Request);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 获取要更新的药材基地客户。
    /// </summary>
    private async Task<CrmHerbBase> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases
            .FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);

        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        return customer;
    }

    /// <summary>
    /// 更新药材基地基础资料。
    /// </summary>
    private static void UpdateBasicInfo(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        var baseName = GetBaseName(request.BaseName, request.HerbBaseName);

        customer.UpdateBasicInfo(
            baseName,
            request.Grade,
            request.Score,
            request.Province,
            request.City,
            request.Area,
            request.Address,
            request.Lat,
            request.Lng,
            request.Remark,
            request.SubjectName);
    }

    /// <summary>
    /// 取兼容旧字段后的基地名称。
    /// </summary>
    private static string GetBaseName(string? baseName, string herbBaseName)
    {
        return string.IsNullOrWhiteSpace(baseName)
            ? herbBaseName
            : baseName;
    }

    /// <summary>
    /// 上级客户变化时更新上级客户。
    /// </summary>
    private static void ApplyParent(CrmHerbBase customer, Guid? parentId)
    {
        if (customer.ParentId != parentId)
        {
            customer.SetParent(parentId);
        }
    }

    /// <summary>
    /// 负责人变化时更新负责人。
    /// </summary>
    private static void ApplyOwner(CrmHerbBase customer, Guid? ownerUserId)
    {
        if (customer.OwnerUserId != ownerUserId)
        {
            customer.AssignOwner(ownerUserId);
        }
    }

    /// <summary>
    /// 来源变化时更新来源信息。
    /// </summary>
    private static void ApplySource(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        if (customer.SourcePlatform != request.SourcePlatform ||
            customer.SourceId != request.SourceId)
        {
            customer.UpdateSource(
                request.SourcePlatform,
                request.SourceId);
        }
    }

    /// <summary>
    /// 请求带主联系人时同步客户主联系人摘要。
    /// </summary>
    private static void ApplyPrimaryContact(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PrimaryContactName) &&
            string.IsNullOrWhiteSpace(request.PrimaryContactPhone))
        {
            return;
        }

        var primaryContactName = string.IsNullOrWhiteSpace(request.PrimaryContactName)
            ? customer.PrimaryContactName
            : request.PrimaryContactName!;
        var primaryContactPhone = string.IsNullOrWhiteSpace(request.PrimaryContactPhone)
            ? customer.PrimaryContactPhone
            : request.PrimaryContactPhone!;

        customer.UpdatePrimaryContact(primaryContactName, primaryContactPhone);
    }

    /// <summary>
    /// 请求带状态且状态变化时更新客户状态。
    /// </summary>
    private static void ApplyStatus(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        if (!string.IsNullOrEmpty(request.Status) && customer.Status != request.Status)
        {
            customer.UpdateStatus(request.Status, request.Remark);
        }
    }
}



