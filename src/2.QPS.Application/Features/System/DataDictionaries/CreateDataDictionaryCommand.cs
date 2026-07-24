using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Interfaces;
using QPS.Domain.Entities.System;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.DataDictionaries;

public record CreateDataDictionaryCommand : IRequest<DataDictionaryDto>
{
    public DataDictionaryCreateRequest Request { get; set; }
}

public class CreateDataDictionaryCommandHandler : IRequestHandler<CreateDataDictionaryCommand, DataDictionaryDto>
{
    private readonly IDbContext _dbContext;

    public CreateDataDictionaryCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataDictionaryDto> Handle(CreateDataDictionaryCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.ParentId.HasValue)
        {
            var parentExists = await _dbContext.SystemDataDictionaries
                .AnyAsync(d => d.Id == request.Request.ParentId.Value, cancellationToken);

            if (!parentExists)
            {
                throw new BusinessException(404, "Parent data dictionary does not exist.");
            }
        }

        var exists = await _dbContext.SystemDataDictionaries
            .AnyAsync(d => d.Code == request.Request.Code, cancellationToken);

        if (exists)
        {
            throw new BusinessException(400, "Code already exists.");
        }

        var dataDictionary = new SystemDataDictionary(
            Guid.NewGuid(),
            request.Request.Code,
            request.Request.Name,
            request.Request.Value,
            request.Request.Description,
            request.Request.SortOrder,
            request.Request.IsActive,
            request.Request.ParentId);

        await _dbContext.SystemDataDictionaries.AddAsync(dataDictionary, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(dataDictionary);
    }

    private static DataDictionaryDto ToDto(SystemDataDictionary dataDictionary)
    {
        return new DataDictionaryDto
        {
            Id = dataDictionary.Id,
            ParentId = dataDictionary.ParentId,
            Code = dataDictionary.Code,
            Name = dataDictionary.Name,
            Value = dataDictionary.Value,
            Description = dataDictionary.Description,
            SortOrder = dataDictionary.SortOrder,
            IsActive = dataDictionary.IsActive
        };
    }
}
