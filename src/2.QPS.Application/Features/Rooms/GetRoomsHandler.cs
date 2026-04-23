using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Rooms;

public class GetRoomsQuery : PaginationRequest, IRequest<PaginationResponse<RoomDto>>
{
    /// <summary>
    /// 房间号
    /// </summary>
    public string? RoomNumber { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, PaginationResponse<RoomDto>>
{
    private readonly IDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetRoomsHandler(IDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaginationResponse<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Rooms.AsNoTracking();

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.RoomNumber))
        {
            query = query.Where(r => r.Name.Contains(request.RoomNumber));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(r => r.Status.ToString().Contains(request.Status));
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
        }

        // 转换为DTO
        var dtoQuery = query.Select(r => new RoomDto
        {
            Id = r.Id,
            RoomNumber = r.Name,
            Status = r.Status.ToString(),
            ShopId = r.ShopId,
            UnitPrice = r.UnitPrice,
            IsEnabled = r.IsEnabled
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}