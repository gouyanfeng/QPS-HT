using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;
using QPS.Domain.Events.Crm;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Crm.CrmHerbBases;

public class UpdateCrmHerbBaseCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public CrmHerbBaseUpdateRequest Request { get; set; } = null!;
}

public class UpdateCrmHerbBaseHandler : IRequestHandler<UpdateCrmHerbBaseCommand, bool>
{
    private readonly IDbContext _dbContext;
    private readonly IPublisher _publisher;

    public UpdateCrmHerbBaseHandler(IDbContext dbContext, IPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<bool> Handle(UpdateCrmHerbBaseCommand request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomer(request.Id, cancellationToken);

        UpdateBasicInfo(customer, request.Request);
        ApplySource(customer, request.Request);
        ApplyPrimaryContact(customer, request.Request);
        ApplyStatus(customer, request.Request);

        await SyncMainProducts(customer.Id, request.Request.MainProducts, cancellationToken);
        await SyncSubjectScaleAsync(customer, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        if (customer.HerbBaseSubjectId.HasValue)
        {
            await _publisher.Publish(new CrmHerbBaseSubjectScoreAffectedEvent(customer.HerbBaseSubjectId.Value), cancellationToken);
        }

        return true;
    }

    private async Task<CrmHerbBase> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.CrmHerbBases.FirstOrDefaultAsync(c => c.Id == customerId && !c.IsDeleted, cancellationToken);
        if (customer == null)
        {
            throw new BusinessException(404, "药材基地不存在");
        }

        return customer;
    }

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
            request.Scale,
            request.Lat,
            request.Lng,
            request.Remark,
            request.SubjectName);
    }

    private static string GetBaseName(string? baseName, string herbBaseName)
    {
        return string.IsNullOrWhiteSpace(baseName)
            ? herbBaseName
            : baseName;
    }

    private static void ApplySource(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        if (customer.SourcePlatform != request.SourcePlatform ||
            customer.SourceId != request.SourceId)
        {
            customer.UpdateSource(request.SourcePlatform, request.SourceId);
        }
    }

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

    private static void ApplyStatus(CrmHerbBase customer, CrmHerbBaseUpdateRequest request)
    {
        if (!string.IsNullOrEmpty(request.Status) && customer.Status != request.Status)
        {
            customer.UpdateStatus(request.Status, request.Remark);
        }
    }

    private async Task SyncMainProducts(Guid herbBaseId, List<string> mainProducts, CancellationToken cancellationToken)
    {
        var oldAttributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == CrmCodes.HerbBaseEntityType &&
                attribute.EntityId == herbBaseId &&
                attribute.AttributeCode == CrmCodes.MainProductAttributeCode)
            .ToListAsync(cancellationToken);
        _dbContext.CrmBusinessEntityAttributes.RemoveRange(oldAttributes);

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

    private async Task SyncSubjectScaleAsync(CrmHerbBase herbBase, CancellationToken cancellationToken)
    {
        if (!herbBase.HerbBaseSubjectId.HasValue)
        {
            return;
        }

        var subject = await _dbContext.CrmHerbBaseSubjects.FirstOrDefaultAsync(item => item.Id == herbBase.HerbBaseSubjectId.Value, cancellationToken);
        if (subject == null)
        {
            return;
        }

        var otherBaseScale = await _dbContext.CrmHerbBases
            .Where(item =>
                item.HerbBaseSubjectId == herbBase.HerbBaseSubjectId.Value &&
                item.Id != herbBase.Id)
            .SumAsync(item => item.Scale ?? 0, cancellationToken);

        subject.UpdateScale(otherBaseScale + (herbBase.Scale ?? 0));
    }
}
