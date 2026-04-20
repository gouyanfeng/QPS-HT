using Microsoft.AspNetCore.Mvc;
using QPS.Infrastructure.Identity;

namespace QPS.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtGenerator _jwtGenerator;

    public AuthController(JwtGenerator jwtGenerator)
    {
        _jwtGenerator = jwtGenerator;
    }

    [HttpPost("login")]
    public ActionResult<string> Login([FromBody] LoginRequest request)
    {
        // 简化的登录逻辑，实际应验证用户名密码
        var token = _jwtGenerator.GenerateToken(Guid.NewGuid(), request.MerchantId, "Admin");
        return Ok(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public Guid MerchantId { get; set; }
}