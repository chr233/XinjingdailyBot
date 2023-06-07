using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.IPC.Middlewares;

/// <summary>
/// 身份验证中间件
/// </summary>
[AppService(LifeTime.Singleton)]
public sealed class ApiAuthenticationMiddleware : IMiddleware
{
    /// <summary>
    /// Header参数名称
    /// </summary>
    public static string HeaderName => "Authentication";
    /// <summary>
    /// Query参数名称
    /// </summary>
    public static string QueryName => "token";
    /// <summary>
    /// 显示名称
    /// </summary>
    public static string FieldName => "User Token";

    private readonly ILogger<ApiAuthenticationMiddleware> _logger;
    private readonly IUserService _userService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="userService"></param>
    public ApiAuthenticationMiddleware(
        ILogger<ApiAuthenticationMiddleware> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    /// <summary>
    /// 验证用户
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var user = await GetAuthenticationStatus(context).ConfigureAwait(false);

        if (user != null && !user.IsBan)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        var response = new GenericResponse {
            Code = HttpStatusCode.Unauthorized,
            Success = false,
        };

        if (user == null)
        {
            response.Message = "Token无效";
        }
        else
        {
            response.Message = "用户已封禁";
        }

        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }

    private async Task<Users?> GetAuthenticationStatus(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Headers.TryGetValue(HeaderName, out var token) && !context.Request.Query.TryGetValue(QueryName, out token))
        {
            return null;
        }

        var user = await _userService.Queryable()
            .Includes(static x => x.Token)
            .FirstAsync(x => x.Token!.APIToken == token);

        if (user != null)
        {
            context.Items.Add("Users", user);
        }

        return user; ;
    }
}