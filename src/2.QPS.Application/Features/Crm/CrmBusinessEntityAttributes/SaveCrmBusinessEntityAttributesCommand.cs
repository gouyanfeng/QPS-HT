using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
using QPS.Application.Features.Crm;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.Crm;

namespace QPS.Application.Features.Crm.CrmBusinessEntityAttributes;

public class SaveCrmBusinessEntityAttributesCommand : IRequest<bool>
{
    public CrmBusinessEntityAttributeSaveRequest Request { get; set; } = null!;
}

public class SaveCrmBusinessEntityAttributesHandler : IRequestHandler<SaveCrmBusinessEntityAttributesCommand, bool>
{
    private readonly IDbContext _dbContext;

    public SaveCrmBusinessEntityAttributesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(SaveCrmBusinessEntityAttributesCommand request, CancellationToken cancellationToken)
    {
        var values = NormalizeValues(request.Request.Values);
        var oldAttributes = await GetOldAttributes(request.Request, cancellationToken);
        _dbContext.CrmBusinessEntityAttributes.RemoveRange(oldAttributes);
        AddNewAttributes(request.Request, values);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RecalculateHerbBaseSubjectScore(request.Request, cancellationToken);

        return true;
    }

    private static List<string> NormalizeValues(IEnumerable<string> values)
    {
        return values
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<CrmBusinessEntityAttribute>> GetOldAttributes(
        CrmBusinessEntityAttributeSaveRequest request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == request.EntityType &&
                attribute.EntityId == request.EntityId &&
                attribute.AttributeCode == request.AttributeCode)
            .ToListAsync(cancellationToken);
    }

    private void AddNewAttributes(CrmBusinessEntityAttributeSaveRequest request, List<string> values)
    {
        var sortOrder = 1;

        foreach (var value in values)
        {
            _dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute(
                request.EntityType,
                request.EntityId,
                request.AttributeCode,
                value,
                sortOrder++));
        }
    }

    private async Task RecalculateHerbBaseSubjectScore(
        CrmBusinessEntityAttributeSaveRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EntityType != CrmCodes.HerbBaseEntityType ||
            request.AttributeCode != CrmCodes.MainProductAttributeCode)
        {
            return;
        }

        var subjectId = await _dbContext.CrmHerbBases
            .Where(item => item.Id == request.EntityId && item.HerbBaseSubjectId.HasValue)
            .Select(item => item.HerbBaseSubjectId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!subjectId.HasValue)
        {
            return;
        }

        var subject = await _dbContext.CrmHerbBaseSubjects.FirstOrDefaultAsync(item => item.Id == subjectId.Value, cancellationToken);
        if (subject == null)
        {
            return;
        }

        var scoreInput = await CrmHerbBaseSubjectScoreInputBuilder.BuildAsync(_dbContext, subject.Id, cancellationToken);
        if (scoreInput != null)
        {
            subject.RecalculateScoreGrade(scoreInput);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
