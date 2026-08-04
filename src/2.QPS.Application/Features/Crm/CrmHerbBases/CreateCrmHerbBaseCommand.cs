using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Exceptions;

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
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// 创建药材基地处理器。
    /// </summary>
    public CreateCrmHerbBaseHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 编排创建药材基地用例。
    /// </summary>
    public async Task<bool> Handle(CreateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        // 编排创建药材基地用例：
        // 创建主体和基地、同步主体主联系人摘要、保存默认流转记录。
        var herbBase = CreateHerbBase(request.Request);
        var (subject, isNewSubject) = await ResolveSubjectAsync(request.Request, herbBase.BaseName, cancellationToken);
        herbBase.SetHerbBaseSubject(subject.Id);

        if (isNewSubject)
        {
            ApplyPrimaryContact(subject, request.Request);
            _dbContext.CrmHerbBaseSubjects.Add(subject);
        }

        _dbContext.CrmHerbBases.Add(herbBase);
        AddMainProducts(herbBase.Id, request.Request.MainProducts);
        if (isNewSubject)
        {
            _dbContext.CrmTransferRecords.Add(CrmTransferRecord.Create(
                CrmCodes.HerbBaseSubjectEntityType,
                subject.Id,
                null,
                request.Request.OwnerUserId,
                GetOperatorUserId(),
                request.Request.Remark.Trim()));
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private void AddMainProducts(Guid herbBaseId, List<string> mainProducts)
    {
        var values = mainProducts
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();
        for (var i = 0; i < values.Count; i++)
        {
            _dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute(
                CrmCodes.HerbBaseEntityType,
                herbBaseId,
                CrmCodes.MainProductAttributeCode,
                values[i],
                i));
        }
    }

    /// <summary>
    /// 根据请求创建药材基地实体。
    /// </summary>
    private static CrmHerbBase CreateHerbBase(CrmHerbBaseCreateRequest request)
    {
        var baseName = GetBaseName(request.BaseName, request.HerbBaseName);

        return CrmHerbBase.Create(
            herbBaseName: baseName,
            grade: request.Grade,
            score: request.Score,
            province: request.Province,
            city: request.City,
            area: request.Area,
            address: request.Address,
            lat: request.Lat,
            lng: request.Lng,
            sourcePlatform: request.SourcePlatform,
            sourceId: request.SourceId,
            ownerUserId: request.OwnerUserId,
            remark: request.Remark,
            subjectName: request.SubjectName,
            scale: request.Scale);
    }

    /// <summary>
    /// 根据基地信息创建或承接基地主体。
    /// </summary>
    private static CrmHerbBaseSubject CreateSubject(CrmHerbBaseCreateRequest request, string baseName)
    {
        var hasSubjectName = !string.IsNullOrWhiteSpace(request.SubjectName);
        return CrmHerbBaseSubject.Create(
            request.SubjectName,
            baseName,
            hasSubjectName ? "UNKNOWN" : "BASE_ONLY",
            request.OwnerUserId,
            CrmCodes.Status.Pending,
            request.Grade,
            request.Score,
            request.Remark);
    }

    /// <summary>
    /// 有主体名称时复用现有主体；没有主体名称时为基地创建独立主体。
    /// </summary>
    private async Task<(CrmHerbBaseSubject Subject, bool IsNew)> ResolveSubjectAsync(
        CrmHerbBaseCreateRequest request,
        string baseName,
        CancellationToken cancellationToken)
    {
        if (request.HerbBaseSubjectId.HasValue)
        {
            var subject = await _dbContext.CrmHerbBaseSubjects
                .FirstOrDefaultAsync(subject => subject.Id == request.HerbBaseSubjectId.Value, cancellationToken);
            return subject == null
                ? throw new BusinessException(404, "药材基地主体不存在")
                : (subject, false);
        }

        if (string.IsNullOrWhiteSpace(request.SubjectName))
        {
            return (CreateSubject(request, baseName), true);
        }

        var normalizedSubjectName = request.SubjectName.Trim().ToUpperInvariant();
        var existingSubject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(subject => subject.NormalizedSubjectName == normalizedSubjectName, cancellationToken);

        return existingSubject == null
            ? (CreateSubject(request, baseName), true)
            : (existingSubject, false);
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
    /// 请求带主联系人时同步主体主联系人摘要。
    /// </summary>
    private static void ApplyPrimaryContact(CrmHerbBaseSubject subject, CrmHerbBaseCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PrimaryContactName) &&
            string.IsNullOrWhiteSpace(request.PrimaryContactPhone))
        {
            return;
        }

        subject.UpdatePrimaryContact(
            request.PrimaryContactName ?? string.Empty,
            request.PrimaryContactPhone ?? string.Empty);
    }

    private Guid? GetOperatorUserId()
    {
        return Guid.TryParse(_currentUserService.UserId, out var operatorUserId)
            ? operatorUserId
            : null;
    }
}



