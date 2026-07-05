using MediatR;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Interfaces;
using QPS.Domain.Exceptions;

namespace QPS.Application.Features.Permissions;

public class GetPermissionQuery : IRequest<PermissionDto>
{
    public Guid Id { get; set; }
}

public class GetPermissionHandler : IRequestHandler<GetPermissionQuery, PermissionDto>
{
    private readonly IDbContext _dbContext;

    public GetPermissionHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PermissionDto> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.SystemPermissions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (permission == null)
        {
            throw new BusinessException(404, "权限不存在");
        }

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
