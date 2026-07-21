using Microsoft.EntityFrameworkCore;
using MediatR;
using QPS.Application.Contracts.System.DataDictionaries;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.DataDictionaries;

public record GetDataDictionaryQuery : IRequest<DataDictionaryDto>
{
    public Guid Id { get; set; }
}

public class GetDataDictionaryQueryHandler : IRequestHandler<GetDataDictionaryQuery, DataDictionaryDto>
{
    private readonly IDbContext _dbContext;

    public GetDataDictionaryQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataDictionaryDto> Handle(GetDataDictionaryQuery request, CancellationToken cancellationToken)
    {
        var dataDictionary = await _dbContext.SystemDataDictionaries
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (dataDictionary == null)
        {
            throw new BusinessException(404, "数据字典不存在");
        }

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