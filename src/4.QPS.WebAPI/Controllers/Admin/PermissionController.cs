using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Permissions;
using QPS.Application.Features.Permissions;

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

    /// <summary>
    /// 获取权限树
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<PermissionTreeNodeDto>>> GetPermissionTree()
    {
        var query = new GetPermissionTreeQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 获取所有角色的权限映射
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, RolePermissionsDto>>> GetRolePermissions()
    {
        var query = new GetRolePermissionsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 获取指定角色的权限
    /// </summary>
    [HttpGet("{roleCode}")]
    public async Task<ActionResult<RolePermissionsDto>> GetRolePermission(string roleCode)
    {
        var query = new GetRolePermissionsQuery { RoleCode = roleCode };
        var result = await _mediator.Send(query);
        if (result.TryGetValue(roleCode, out var dto))
            return Ok(dto);
        return Ok(new RolePermissionsDto());
    }

    /// <summary>
    /// 更新角色权限
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<bool>> UpdateRolePermission([FromBody] UpdateRolePermissionsRequest request)
    {
        var command = new UpdateRolePermissionsCommand
        {
            Role = request.Role,
            Permissions = request.Permissions
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
