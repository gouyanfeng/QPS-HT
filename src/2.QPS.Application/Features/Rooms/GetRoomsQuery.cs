using MediatR;
using QPS.Application.Contracts.Rooms;
using QPS.Application.Extensions;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Rooms;

public class GetRoomsQuery : PaginationRequest, IRequest<PaginationResponse<RoomDto>>
{
    public string? RoomNumber { get; set; }
    public string? Status { get; set; }
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
        IQueryable<Room> query = _dbContext.Rooms.AsNoTracking();

        if (!string.IsNullOrEmpty(request.RoomNumber))
        {
            query = query.Where(r => r.Name.Contains(request.RoomNumber));
        }

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<RoomStatus>(request.Status, true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
        }

        var dtoQuery = query
            .Include(r => r.Shop)
            .Include(r => r.RoomImages)
            .Include(r => r.RoomTags)
                .ThenInclude(rt => rt.Tag)
            .Include(r => r.RoomPlans)
                .ThenInclude(rp => rp.Plan)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.Name,
                Status = r.Status.ToChinese(),
                ShopId = r.ShopId,
                ShopName = r.Shop.Name,
                ShopAddress = r.Shop.Address,
                UnitPrice = r.UnitPrice,
                IsEnabled = r.IsEnabled,
                Images = r.RoomImages.Select(ri => new RoomImageItemDto
                {
                    Id = ri.Id,
                    RoomId = ri.RoomId,
                    Url = ri.ImageUrl,
                    SortOrder = ri.SortOrder
                }).ToList(),
                Tags = r.RoomTags.Select(rt => new RoomTagItemDto
                {
                    Id = rt.Id,
                    RoomId = rt.RoomId,
                    TagId = rt.TagId,
                    TagName = rt.Tag.TagName
                }).ToList(),
                Plans = r.RoomPlans.Select(rp => new RoomPlanItemDto
                {
                    Id = rp.Id,
                    RoomId = rp.RoomId,
                    PlanId = rp.PlanId,
                    PlanName = rp.Plan.Name,
                    Price = rp.Plan.Price
                }).ToList()
            });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}