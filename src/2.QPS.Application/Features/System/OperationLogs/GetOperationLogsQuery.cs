using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.System.OperationLogs;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;

namespace QPS.Application.Features.System.OperationLogs;

public class GetOperationLogsQuery : PaginationRequest, IRequest<PaginationResponse<OperationLogDto>>
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionType { get; set; }
    public string? OperatorName { get; set; }
    public string? RequestPath { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
}

public class GetOperationLogsQueryHandler : IRequestHandler<GetOperationLogsQuery, PaginationResponse<OperationLogDto>>
{
    private readonly IDbContext _dbContext;

    public GetOperationLogsQueryHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<OperationLogDto>> Handle(
        GetOperationLogsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemOperationLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(log => log.EntityType.Contains(request.EntityType));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            query = query.Where(log => log.EntityId.Contains(request.EntityId));
        }

        if (!string.IsNullOrWhiteSpace(request.ActionType))
        {
            query = query.Where(log => log.ActionType == request.ActionType);
        }

        if (!string.IsNullOrWhiteSpace(request.OperatorName))
        {
            query = query.Where(log => log.OperatorName.Contains(request.OperatorName));
        }

        if (!string.IsNullOrWhiteSpace(request.RequestPath))
        {
            query = query.Where(log => log.RequestPath.Contains(request.RequestPath));
        }

        if (request.StartAt.HasValue)
        {
            query = query.Where(log => log.CreatedAt >= request.StartAt.Value);
        }

        if (request.EndAt.HasValue)
        {
            query = query.Where(log => log.CreatedAt <= request.EndAt.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var logs = await query
            .OrderByDescending(log => log.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(log => new OperationLogDto
            {
                Id = log.Id,
                CreatedAt = log.CreatedAt,
                ActionType = log.ActionType,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                OperatorName = log.OperatorName,
                RequestPath = log.RequestPath,
                IpAddress = log.IpAddress,
                ChangeJson = log.ChangeJson
            })
            .ToListAsync(cancellationToken);

        return new PaginationResponse<OperationLogDto>(logs, totalCount, request.Page, request.PageSize);
    }
}
