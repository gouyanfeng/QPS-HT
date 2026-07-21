using Microsoft.EntityFrameworkCore;
using MediatR;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.DataDictionaries;

public record UpdateDataDictionaryCommand : IRequest<DataDictionaryDto>
{
    public Guid Id { get; set; }
    public DataDictionaryUpdateRequest Request { get; set; }
}

public class UpdateDataDictionaryCommandHandler : IRequestHandler<UpdateDataDictionaryCommand, DataDictionaryDto>
{
    private readonly IDbContext _dbContext;

    public UpdateDataDictionaryCommandHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataDictionaryDto> Handle(UpdateDataDictionaryCommand request, CancellationToken cancellationToken)
    {
        var dataDictionary = await _dbContext.SystemDataDictionaries
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (dataDictionary == null)
        {
            throw new BusinessException(404, "数据字典不存在");
        }

        if (request.Request.ParentId.HasValue)
        {
            var parent = await _dbContext.SystemDataDictionaries
                .FirstOrDefaultAsync(d => d.Id == request.Request.ParentId.Value, cancellationToken);
            if (parent == null)
            {
                throw new BusinessException(404, "父级数据字典不存在");
            }
        }

        dataDictionary.Update(
            request.Request.Name,
            request.Request.Value,
            request.Request.Description,
            request.Request.SortOrder,
            request.Request.IsActive,
            request.Request.ParentId
        );

        await _dbContext.SaveChangesAsync(cancellationToken);

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