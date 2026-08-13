using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

public class CreateCrmHerbBaseCommand : IRequest<bool>
{
    public CrmHerbBaseCreateRequest Request { get; set; } = null!;
}

public class CreateCrmHerbBaseHandler : IRequestHandler<CreateCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public CreateCrmHerbBaseHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(CreateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var herbBase = CreateHerbBase(request.Request);
        var (subject, isNewSubject) = await ResolveSubjectAsync(request.Request, herbBase.BaseName, cancellationToken);
        herbBase.SetHerbBaseSubject(subject.Id);
        await SyncSubjectScaleAsync(subject, herbBase.Scale ?? 0, isNewSubject, cancellationToken);

        if (isNewSubject)
        {
            ApplyPrimaryContact(subject, request.Request);
            _dbContext.CrmHerbBaseSubjects.Add(subject);
        }

        _dbContext.CrmHerbBases.Add(herbBase);
        AddMainProducts(herbBase.Id, request.Request.MainProducts);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (herbBase.HerbBaseSubjectId.HasValue)
        {
            await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(herbBase.HerbBaseSubjectId.Value), cancellationToken);
        }

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
            ownerUserId: null,
            remark: request.Remark,
            subjectName: request.SubjectName,
            scale: request.Scale);
    }

    private static CrmHerbBaseSubject CreateSubject(CrmHerbBaseCreateRequest request, string baseName)
    {
        var hasSubjectName = !string.IsNullOrWhiteSpace(request.SubjectName);
        return CrmHerbBaseSubject.Create(
            request.SubjectName,
            baseName,
            hasSubjectName ? "UNKNOWN" : "BASE_ONLY",
            null,
            CrmCodes.Status.Pending,
            request.Grade,
            request.Score,
            request.Remark,
            request.Scale);
    }

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

        var subjectName = request.SubjectName.Trim();
        var existingSubject = await _dbContext.CrmHerbBaseSubjects
            .FirstOrDefaultAsync(subject => subject.SubjectName == subjectName, cancellationToken);

        return existingSubject == null
            ? (CreateSubject(request, baseName), true)
            : (existingSubject, false);
    }

    private static string GetBaseName(string? baseName, string herbBaseName)
    {
        return string.IsNullOrWhiteSpace(baseName)
            ? herbBaseName
            : baseName;
    }

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

    private async Task SyncSubjectScaleAsync(
        CrmHerbBaseSubject subject,
        decimal newBaseScale,
        bool isNewSubject,
        CancellationToken cancellationToken)
    {
        var existingScale = isNewSubject
            ? 0
            : await _dbContext.CrmHerbBases
                .Where(herbBase => herbBase.HerbBaseSubjectId == subject.Id)
                .SumAsync(herbBase => herbBase.Scale ?? 0, cancellationToken);

        subject.UpdateScale(existingScale + newBaseScale);
    }
}
