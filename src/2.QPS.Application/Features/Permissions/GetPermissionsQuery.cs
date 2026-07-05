using MediatR;
using Microsoft.EntityFrameworkCore;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Interfaces;
using QPS.Application.Pagination;

namespace QPS.Application.Features.Permissions;

public class GetPermissionsQuery : PaginationRequest, IRequest<PaginationResponse<PermissionDto>>
{
    public string? PermissionCode { get; set; }
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }
}

public class GetPermissionsHandler : IRequestHandler<GetPermissionsQuery, PaginationResponse<PermissionDto>>
{
    private readonly IDbContext _dbContext;

    public GetPermissionsHandler(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginationResponse<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SystemPermissions.AsQueryable();

        if (!string.IsNullOrEmpty(request.PermissionCode))
        {
            query = query.Where(p => p.PermissionCode.Contains(request.PermissionCode));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(p => p.Name.Contains(request.Name));
        }

        if (request.ParentId.HasValue)
        {
            query = query.Where(p => p.ParentId == request.ParentId.Value);
        }

        var dtoQuery = query.Select(p => new PermissionDto
        {
            Id = p.Id,
            MerchantId = p.MerchantId,
            PermissionCode = p.PermissionCode,
            Name = p.Name,
            ParentId = p.ParentId
        });

        return await dtoQuery.ToPaginationResponseAsync(request);
    }
}
