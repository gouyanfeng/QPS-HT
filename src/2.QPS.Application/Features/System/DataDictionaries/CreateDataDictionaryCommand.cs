using Microsoft.EntityFrameworkCore;
using MediatR;
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
    private readonly ICurrentUserService _currentUserService;

    public CreateDataDictionaryCommandHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<DataDictionaryDto> Handle(CreateDataDictionaryCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;

        if (request.Request.ParentId.HasValue)
        {
            var parent = await _dbContext.SystemDataDictionaries
                .FirstOrDefaultAsync(d => d.Id == request.Request.ParentId.Value && d.MerchantId == merchantId, cancellationToken);
            if (parent == null)
            {
                throw new BusinessException(404, "父级数据字典不存在");
            }
        }

        var exists = await _dbContext.SystemDataDictionaries
            .AnyAsync(d => d.Code == request.Request.Code && d.MerchantId == merchantId, cancellationToken);
        if (exists)
        {
            throw new BusinessException(400, "编码已存在");
        }

        var dataDictionary = new SystemDataDictionary(
            Guid.NewGuid(),
            request.Request.Code,
            request.Request.Name,
            request.Request.Value,
            request.Request.Description,
            request.Request.SortOrder,
            request.Request.IsActive,
            merchantId,
            request.Request.ParentId
        );

        await _dbContext.SystemDataDictionaries.AddAsync(dataDictionary, cancellationToken);
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
            IsActive = dataDictionary.IsActive,
            MerchantId = dataDictionary.MerchantId
        };
    }
}