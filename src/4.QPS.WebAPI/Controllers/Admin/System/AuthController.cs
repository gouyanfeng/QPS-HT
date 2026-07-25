using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using QPS.Application.Contracts.System.Auth;
using QPS.Application.Features.System.Auth;

namespace QPS.WebAPI.Controllers.Admin.System;

[ApiController]
[Route("api/admin/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand { Request = request };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout([FromBody] LogoutRequest request)
    {
        var command = new LogoutCommand { Request = request };
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// 获取当前登录用户权限代码列表
    /// </summary>
    [HttpGet("user-permissions")]
    public async Task<ActionResult<UserPermissionsResponse>> GetUserPermissions()
    {
        var query = new GetUserPermissionsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 修改当前登录用户密码
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand
        {
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}