using Microsoft.EntityFrameworkCore;
using MediatR;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.System.DataDictionaries;

public record DeleteDataDictionaryCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}

public class DeleteDataDictionaryCommandHandler : IRequestHandler<DeleteDataDictionaryCommand, Unit>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteDataDictionaryCommandHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteDataDictionaryCommand request, CancellationToken cancellationToken)
    {
        var merchantId = _currentUserService.MerchantId;

        var dataDictionary = await _dbContext.SystemDataDictionaries
            .FirstOrDefaultAsync(d => d.Id == request.Id && d.MerchantId == merchantId, cancellationToken);
        if (dataDictionary == null)
        {
            throw new BusinessException(404, "数据字典不存在");
        }

        var hasChildren = await _dbContext.SystemDataDictionaries
            .AnyAsync(d => d.ParentId == request.Id && d.MerchantId == merchantId, cancellationToken);
        if (hasChildren)
        {
            throw new BusinessException(400, "存在子节点，无法删除");
        }

        _dbContext.SystemDataDictionaries.Remove(dataDictionary);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}