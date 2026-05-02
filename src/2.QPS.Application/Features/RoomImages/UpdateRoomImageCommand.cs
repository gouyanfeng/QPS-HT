using MediatR;
using QPS.Application.Contracts.RoomImages;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.RoomImages;

public class UpdateRoomImageCommand : IRequest<RoomImageDto>
{
    public Guid Id { get; set; }
    public RoomImageUpdateRequest Request { get; set; }
}

public class UpdateRoomImageHandler : IRequestHandler<UpdateRoomImageCommand, RoomImageDto>
{
    private readonly IDbContext _dbContext;

    public UpdateRoomImageHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RoomImageDto> Handle(UpdateRoomImageCommand request, CancellationToken cancellationToken)
    {
        var image = await _dbContext.RoomImages.FindAsync(request.Id, cancellationToken);

        if (image == null)
        {
            throw new BusinessException(404, "房间图片不存在");
        }

        image.Update(
            request.Request.ImageUrl,
            request.Request.IsMain,
            request.Request.SortOrder
        );

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