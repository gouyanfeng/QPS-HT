using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.OperationLogs;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.OperationLogs;

public class GetOperationLogsQuery : IRequest<PaginationResponse<OperationLogDto>>
{
    public OperationLogQueryRequest Request { get; set; } = new();
}

public class GetOperationLogsHandler : IRequestHandler<GetOperationLogsQuery, PaginationResponse<OperationLogDto>>
{
    private readonly IDbContext _dbContext;

    public GetOperationLogsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<OperationLogDto>> Handle(GetOperationLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemOperationLogs.AsNoTracking();
        var filter = request.Request;

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
        {
            query = query.Where(log => log.EntityType == filter.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            query = query.Where(log => log.EntityId == filter.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(filter.ActionType))
        {
            query = query.Where(log => log.ActionType == filter.ActionType);
        }

        if (!string.IsNullOrWhiteSpace(filter.OperatorUserId))
        {
            query = query.Where(log => log.OperatorUserId == filter.OperatorUserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.OperatorName))
        {
            query = query.Where(log => log.OperatorName.Contains(filter.OperatorName));
        }

        if (!string.IsNullOrWhiteSpace(filter.RequestPath))
        {
            query = query.Where(log => log.RequestPath.Contains(filter.RequestPath));
        }

        if (filter.StartAt.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= filter.StartAt.Value);
        }

        if (filter.EndAt.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= filter.EndAt.Value);
        }

        var dtoQuery = query.Select(log => new OperationLogDto
        {
            Id = log.Id,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            ActionType = log.ActionType,
            ChangeJson = log.ChangeJson,
            OperatorUserId = log.OperatorUserId,
            OperatorName = log.OperatorName,
            RequestPath = log.RequestPath,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            CreatedAt = log.CreatedAt
        });

        return await dtoQuery.ToPaginationResponseAsync(filter);
    }
}
