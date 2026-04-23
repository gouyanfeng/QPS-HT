using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPS.Application.Contracts.Users;
using QPS.Application.Features.Users;
using QPS.Application.Pagination;
using System;

namespace QPS.WebAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResponse<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortField = "Username",
        [FromQuery] string sortDirection = "Ascending",
        [FromQuery] string? username = null,
        [FromQuery] string? realName = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SortField = sortField,
            SortDirection = sortDirection,
            Username = username,
            RealName = realName,
            IsActive = isActive,
            StartDate = startDate,
            EndDate = endDate
        };
        var users = await _mediator.Send(query);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _mediator.Send(new GetUserQuery { Id = id });
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateRequest request)
    {
        var user = await _mediator.Send(new CreateUserCommand { Request = request });
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UserUpdateRequest request)
    {
        var user = await _mediator.Send(new UpdateUserCommand { Id = id, Request = request });
        return Ok(user);
    }


}