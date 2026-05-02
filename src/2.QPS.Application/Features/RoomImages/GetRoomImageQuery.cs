using MediatR;
using QPS.Application.Contracts.RoomImages;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.RoomImages;

public class GetRoomImageQuery : IRequest<RoomImageDto>
{
    public Guid Id { get; set; }
}

public class GetRoomImageHandler : IRequestHandler<GetRoomImageQuery, RoomImageDto>
{
    private readonly IDbContext _dbContext;

    public GetRoomImageHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomImageDto> Handle(GetRoomImageQuery request, CancellationToken cancellationToken)
    {
        var image = await _dbContext.RoomImages.FindAsync(request.Id, cancellationToken);

        if (image == null)
        {
            throw new BusinessException(404, "房间图片不存在");
        }

        return new RoomImageDto
        {
            Id = image.Id,
            RoomId = image.RoomId,
            ImageUrl = image.ImageUrl,
            IsMain = image.IsMain,
            SortOrder = image.SortOrder
        };
    }
}