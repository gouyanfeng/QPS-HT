using MediatR;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmHerbBases;

/// <summary>
/// 创建药材基地命令
/// </summary>
public class CreateCrmHerbBaseCommand : IRequest<bool>
{
    public CrmHerbBaseCreateRequest Request { get; set; } = null!;
}

/// <summary>
/// 创建药材基地处理器
/// </summary>
public class CreateCrmHerbBaseHandler : IRequestHandler<CreateCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// 创建药材基地处理器。
    /// </summary>
    public CreateCrmHerbBaseHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 编排创建药材基地用例。
    /// </summary>
    public async Task<bool> Handle(CreateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        // 编排创建药材基地用例：
        // 创建客户实体、同步主联系人摘要、保存。
        var customer = CreateCustomer(request.Request);

        ApplyPrimaryContact(customer, request.Request);

        _dbContext.CrmHerbBases.Add(customer);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// 根据请求创建药材基地实体。
    /// </summary>
    private static CrmHerbBase CreateCustomer(CrmHerbBaseCreateRequest request)
    {
        var baseName = GetBaseName(request.BaseName, request.HerbBaseName);

        return CrmHerbBase.Create(
            baseName,
            request.Grade,
            request.Score,
            request.Province,
            request.City,
            request.Area,
            request.Address,
            request.Lat,
            request.Lng,
            request.SourcePlatform,
            request.SourceId,
            request.OwnerUserId,
            request.Remark,
            request.ParentId,
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
    /// 请求带主联系人时同步客户主联系人摘要。
    /// </summary>
    private static void ApplyPrimaryContact(CrmHerbBase customer, CrmHerbBaseCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PrimaryContactName) &&
            string.IsNullOrWhiteSpace(request.PrimaryContactPhone))
        {
            return;
        }

        customer.UpdatePrimaryContact(
            request.PrimaryContactName ?? string.Empty,
            request.PrimaryContactPhone ?? string.Empty);
    }
}



