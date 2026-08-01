using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Crm;
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
        var values = request.Request.Values
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var oldAttributes = await _dbContext.CrmBusinessEntityAttributes
            .Where(attribute =>
                attribute.EntityType == request.Request.EntityType &&
                attribute.EntityId == request.Request.EntityId &&
                attribute.AttributeCode == request.Request.AttributeCode)
            .ToListAsync(cancellationToken);

        _dbContext.CrmBusinessEntityAttributes.RemoveRange(oldAttributes);

        var sortOrder = 1;
        foreach (var value in values)
        {
            _dbContext.CrmBusinessEntityAttributes.Add(new CrmBusinessEntityAttribute(
                request.Request.EntityType,
                request.Request.EntityId,
                request.Request.AttributeCode,
                value,
                sortOrder++));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
