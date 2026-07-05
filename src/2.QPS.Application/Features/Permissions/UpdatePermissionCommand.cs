using MediatR;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Permissions;

public class UpdatePermissionCommand : IRequest<PermissionDto>
{
    public Guid Id { get; set; }
    public PermissionUpdateRequest Request { get; set; }
}

public class UpdatePermissionHandler : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    private readonly IDbContext _dbContext;

    public UpdatePermissionHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.SystemPermissions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (permission == null)
        {
            throw new BusinessException(404, "权限不存在");
        }

        permission.Update(request.Request.PermissionCode, request.Request.Name, request.Request.ParentId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PermissionDto
        {
            Id = permission.Id,
            MerchantId = permission.MerchantId,
            PermissionCode = permission.PermissionCode,
            Name = permission.Name,
            ParentId = permission.ParentId
        };
    }
}
