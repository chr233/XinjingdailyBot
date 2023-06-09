using Microsoft.Extensions.Options;
using System.Configuration;
using System.Net;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
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
    public static string FieldName => "UserToken";

    private readonly ILogger<ApiAuthenticationMiddleware> _logger;
    private readonly IUserService _userService;
    private readonly IUserTokenService _userTokenService;
    private readonly GroupRepository _groupRepository;
    private readonly HashSet<long>? _superAdmins;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="userService"></param>
    /// <param name="userTokenService"></param>
    /// <param name="groupRepository"></param>
    /// <param name="configuration"></param>
    public ApiAuthenticationMiddleware(
        ILogger<ApiAuthenticationMiddleware> logger,
        IUserService userService,
        IUserTokenService userTokenService,
        GroupRepository groupRepository,
        IOptions<OptionsSetting> configuration)
    {
        _logger = logger;
        _userService = userService;
        _userTokenService = userTokenService;
        _groupRepository = groupRepository;
        _superAdmins = configuration.Value.Bot.SuperAdmins;
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
            Message = (user == null) ? "Token无效" : "用户已封禁",
        };

        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }

    private async Task<Users?> GetAuthenticationStatus(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if ((!context.Request.Headers.TryGetValue(HeaderName, out var token) && !context.Request.Query.TryGetValue(QueryName, out token)) || !Guid.TryParse(token, out var guid))
        {
            return null;
        }

        var dbUser = await _userTokenService.VerifyToken(guid);

        if (dbUser != null && !dbUser.IsBan)
        {
            //如果是配置文件中指定的管理员就覆盖用户组权限
            if (_superAdmins?.Contains(dbUser.UserID) ?? false)
            {
                dbUser.GroupID = _groupRepository.GetMaxGroupId();
            }

            //根据GroupID设置用户权限信息 (封禁用户区别对待)
            var group = _groupRepository.GetGroupById(dbUser.GroupID);

            if (group != null)
            {
                dbUser.Right = group.DefaultRight;
                context.Items.Add("Users", dbUser);
            }
            else
            {
                _logger.LogError("读取用户 {dbUser} 权限组 {GroupID} 失败", dbUser, dbUser.GroupID);
                return null;
            }
        }

        return dbUser;
    }
}
