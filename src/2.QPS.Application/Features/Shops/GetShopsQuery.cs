using MediatR;
using QPS.Application.Contracts.Shops;
using QPS.Application.Contracts.RoomImages;
using QPS.Application.Contracts.Tags;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.Shops;

public class GetShopsQuery : PaginationRequest, IRequest<PaginationResponse<ShopDto>>
{
    public string? Name { get; set; }
}

public class GetShopsHandler : IRequestHandler<GetShopsQuery, PaginationResponse<ShopDto>>
{
    private readonly IDbContext _dbContext;

    public GetShopsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<ShopDto>> Handle(GetShopsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Shops.AsNoTracking()
            .Include(s => s.Rooms)
            .ThenInclude(r => r.RoomTags)
            .ThenInclude(rt => rt.Tag)
            .Include(s => s.Rooms)
            .ThenInclude(r => r.RoomImages)
            .Where(s => string.IsNullOrEmpty(request.Name) || s.Name.Contains(request.Name));

        var shopDtos = query.Select(s => new ShopDto
        {
            Id = s.Id,
            Name = s.Name,
            Address = s.Address,
            Phone = s.Phone,
            OpeningTime = s.OpeningTime,
            ClosingTime = s.ClosingTime,
            AutoPowerOffDelay = s.AutoPowerOffDelay,
            Rooms = s.Rooms.Select(r => new RoomSummaryDto
            {
                Id = r.Id,
                RoomId = r.Id,
                Name = r.Name,
                PricePerHour = r.UnitPrice,
                IsAvailable = r.IsEnabled && r.Status == RoomStatus.Idle,
                Tags = r.RoomTags.Select(rt => new TagDto
                {
                    Id = rt.Tag.Id,
                    TagName = rt.Tag.TagName
                }).ToList(),
                Images = r.RoomImages.OrderBy(ri => ri.SortOrder).Select(ri => new RoomImageDto
                {
                    Id = ri.Id,
                    RoomId = ri.RoomId,
                    ImageUrl = ri.ImageUrl,
                    IsMain = ri.IsMain,
                    SortOrder = ri.SortOrder
                }).ToList()
            }).ToList()
        });

        return await shopDtos.ToPaginationResponseAsync(request);
    }
}