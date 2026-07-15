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
    private readonly ICurrentUserService _currentUserService;

    public GetDataDictionaryQueryHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DataDictionaryDto> Handle(GetDataDictionaryQuery request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;

        var dataDictionary = await _dbContext.SystemDataDictionaries
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.MerchantId == merchantId, cancellationToken);
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
            IsActive = dataDictionary.IsActive,
            MerchantId = dataDictionary.MerchantId
        };
    }
}