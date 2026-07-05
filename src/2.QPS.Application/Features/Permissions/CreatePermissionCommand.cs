using MediatR;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Interfaces;
using QPS.Domain.Entities;

namespace QPS.Application.Features.Permissions;

public class CreatePermissionCommand : IRequest<PermissionDto>
{
    public PermissionCreateRequest Request { get; set; }
}

public class CreatePermissionHandler : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    private readonly IDbContext _dbContext;

    public CreatePermissionHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = new SystemPermission(
            request.Request.PermissionCode,
            request.Request.Name,
            request.Request.ParentId
        );

        _dbContext.SystemPermissions.Add(permission);
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
