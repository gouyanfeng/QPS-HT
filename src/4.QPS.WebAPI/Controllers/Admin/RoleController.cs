using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Roles;
using QPS.Application.Features.Roles;
using QPS.Application.Pagination;

namespace QPS.WebAPI.Controllers.Admin;

/// <summary>
/// 角色控制器
/// </summary>
[Route("api/admin/roles")]
[ApiController]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mediator">中介者</param>
    public RoleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="sortField">排序字段</param>
    /// <param name="sortDirection">排序方向</param>
    /// <param name="name">角色名称</param>
    /// <param name="code">角色代码</param>
    /// <returns>角色列表</returns>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<RoleDto>>> GetRoles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Name",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? name = null,
        [FromQuery] string? code = null)
    {
        var query = new GetRolesQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Name = name,
            Code = code
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色详情</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(Guid id)
    {
        var query = new GetRoleQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    /// <param name="request">创建角色请求</param>
    /// <returns>创建的角色</returns>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] RoleCreateRequest request)
    {
        var command = new CreateRoleCommand { Request = request };
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetRole), new { id = result.Id }, result);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <param name="request">更新角色请求</param>
    /// <returns>更新后的角色</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(Guid id, [FromBody] RoleUpdateRequest request)
    {
        var command = new UpdateRoleCommand { Id = id, Request = request };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}