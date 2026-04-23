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
    /// 设备序列号
    /// </summary>
    public string? DeviceSn { get; set; }
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
        var merchantId = _currentUserService.MerchantId;
        var query = _dbContext.Rooms.Where(r => r.MerchantId == merchantId);

        // 应用查询条件
        if (!string.IsNullOrEmpty(request.RoomNumber))
        {
            query = query.Where(r => r.Name.Contains(request.RoomNumber));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(r => r.Status.ToString().Contains(request.Status));
        }

        if (!string.IsNullOrEmpty(request.DeviceSn))
        {
            query = query.Where(r => r.DeviceSn.Contains(request.DeviceSn));
        }

        // 转换为DTO
        var dtoQuery = query.Select(r => new RoomDto
        {
            Id = r.Id,
            RoomNumber = r.Name,
            Status = r.Status.ToString(),
            MqttTopic = r.MqttTopic,
            ShopId = r.ShopId,
            DeviceSn = r.DeviceSn
        });

        // 执行分页查询
        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}