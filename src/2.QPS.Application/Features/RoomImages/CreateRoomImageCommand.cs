using MediatR;
using QPS.Application.Contracts.RoomImages;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.RoomImages;

public class CreateRoomImageCommand : IRequest<RoomImageDto>
{
    public RoomImageCreateRequest Request { get; set; }
}

public class CreateRoomImageHandler : IRequestHandler<CreateRoomImageCommand, RoomImageDto>
{
    private readonly IDbContext _dbContext;

    public CreateRoomImageHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomImageDto> Handle(CreateRoomImageCommand request, CancellationToken cancellationToken)
    {
        var image = new RoomImage(
            request.Request.RoomId,
            request.Request.ImageUrl,
            request.Request.IsMain,
            request.Request.SortOrder
        );

        _dbContext.RoomImages.Add(image);
        await _dbContext.SaveChangesAsync(cancellationToken);

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