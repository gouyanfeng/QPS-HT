using MediatR;
using QPS.Application.Contracts.RoomImages;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.RoomImages;

public class GetRoomImagesQuery : IRequest<List<RoomImageDto>>
{
    public Guid RoomId { get; set; }
}

public class GetRoomImagesHandler : IRequestHandler<GetRoomImagesQuery, List<RoomImageDto>>
{
    private readonly IDbContext _dbContext;

    public GetRoomImagesHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomImageDto>> Handle(GetRoomImagesQuery request, CancellationToken cancellationToken)
    {
        var images = await _dbContext.RoomImages
            .AsNoTracking()
            .Where(ri => ri.RoomId == request.RoomId)
            .OrderBy(ri => ri.SortOrder)
            .Select(ri => new RoomImageDto
            {
                Id = ri.Id,
                RoomId = ri.RoomId,
                ImageUrl = ri.ImageUrl,
                IsMain = ri.IsMain,
                SortOrder = ri.SortOrder
            })
            .ToListAsync(cancellationToken);

        return images;
    }
}