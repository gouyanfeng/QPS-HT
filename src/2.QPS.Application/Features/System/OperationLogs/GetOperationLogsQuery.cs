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
        var query = _dbContext.SystemErrorLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.ActionType) &&
            !string.Equals(request.ActionType, "Error", StringComparison.OrdinalIgnoreCase))
        {
            return new PaginationResponse<OperationLogDto>(new List<OperationLogDto>(), 0, request.Page, request.PageSize);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType) || !string.IsNullOrWhiteSpace(request.EntityId))
        {
            return new PaginationResponse<OperationLogDto>(new List<OperationLogDto>(), 0, request.Page, request.PageSize);
        }

        if (!string.IsNullOrWhiteSpace(request.OperatorName))
        {
            query = query.Where(log => log.Username.Contains(request.OperatorName));
        }

        if (!string.IsNullOrWhiteSpace(request.RequestPath))
        {
            query = query.Where(log => log.RequestUrl.Contains(request.RequestPath));
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
                ActionType = "Error",
                EntityType = log.ErrorType,
                EntityId = string.Empty,
                OperatorName = log.Username,
                RequestPath = log.RequestUrl,
                IpAddress = log.IpAddress,
                ChangeJson = "{}"
            })
            .ToListAsync(cancellationToken);

        return new PaginationResponse<OperationLogDto>(logs, totalCount, request.Page, request.PageSize);
    }
}
