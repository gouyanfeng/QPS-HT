using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Features.Permissions;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

[Route("api/admin/permissions")]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<PermissionDto>>> GetPermissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "PermissionCode",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? permissionCode = null,
        [FromQuery] string? name = null,
        [FromQuery] Guid? parentId = null)
    {
        var query = new GetPermissionsQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            PermissionCode = permissionCode,
            Name = name,
            ParentId = parentId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PermissionDto>> GetPermission(Guid id)
    {
        var query = new GetPermissionQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PermissionDto>> CreatePermission([FromBody] PermissionCreateRequest request)
    {
        var command = new CreatePermissionCommand { Request = request };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPermission), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PermissionDto>> UpdatePermission(Guid id, [FromBody] PermissionUpdateRequest request)
    {
        var command = new UpdatePermissionCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePermission(Guid id)
    {
        var command = new DeletePermissionCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
