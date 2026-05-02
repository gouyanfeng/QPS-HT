using MediatR;
using QPS.Application.Contracts.RoomTags;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QPS.Application.Features.RoomTags;

public class GetRoomTagsQuery : IRequest<List<RoomTagDto>>
{
    public Guid? RoomId { get; set; }
    public Guid? TagId { get; set; }
}

public class GetRoomTagsHandler : IRequestHandler<GetRoomTagsQuery, List<RoomTagDto>>
{
    private readonly IDbContext _dbContext;

    public GetRoomTagsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomTagDto>> Handle(GetRoomTagsQuery request, CancellationToken cancellationToken)
    {
        var query = from mapping in _dbContext.RoomTags
                    join room in _dbContext.Rooms on mapping.RoomId equals room.Id into roomJoin
                    from room in roomJoin.DefaultIfEmpty()
                    join tag in _dbContext.Tags on mapping.TagId equals tag.Id into tagJoin
                    from tag in tagJoin.DefaultIfEmpty()
                    select new RoomTagDto
                    {
                        Id = mapping.Id,
                        RoomId = mapping.RoomId,
                        RoomNumber = room != null ? room.Name : null,
                        TagId = mapping.TagId,
                        TagName = tag != null ? tag.TagName : null
                    };

        if (request.RoomId.HasValue)
        {
            query = query.Where(m => m.RoomId == request.RoomId.Value);
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(m => m.TagId == request.TagId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}