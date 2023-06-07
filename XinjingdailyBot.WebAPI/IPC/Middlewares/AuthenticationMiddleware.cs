using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.Security.Claims;

namespace ArchiSteamFarm.IPC.Middlewares;

/// <summary>
/// 
/// </summary>
public sealed class AuthenticationMiddleware : IMiddleware
{
    public const string SchemeName = "MyAuth";

    AuthenticationScheme _scheme;
    HttpContext _context;

    /// <summary>
    /// 初始化认证
    /// </summary>
    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _scheme = scheme;
        _context = context;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 认证处理
    /// </summary>
    public Task<AuthenticateResult> AuthenticateAsync()
    {
        var req = _context.Request.Query;
        var isLogin = req["isLogin"].FirstOrDefault();

        if (isLogin != "true")
        {
            return Task.FromResult(AuthenticateResult.Fail("未登陆"));
        }

        var ticket = GetAuthTicket("test", "test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    AuthenticationTicket GetAuthTicket(string name, string role)
    {
        var claimsIdentity = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role),
    }, "My_Auth");

        var principal = new ClaimsPrincipal(claimsIdentity);
        return new AuthenticationTicket(principal, _scheme.Name);
    }

    /// <summary>
    /// 权限不足时的处理
    /// </summary>
    public Task ForbidAsync(AuthenticationProperties properties)
    {
        _context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 未登录时的处理
    /// </summary>
    public Task ChallengeAsync(AuthenticationProperties properties)
    {
        _context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return Task.CompletedTask;
    }

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        return Task.CompletedTask;
    }
}
