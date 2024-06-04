using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Net;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Repository;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.Authorization;

/// <summary>
/// 授权校验访问
/// 如果跳过授权登录在Action 或controller加上 AllowAnonymousAttribute
/// </summary>

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class VerifyAttribute : Attribute, IAsyncAuthorizationFilter
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

    /// <summary>
    /// 鉴权验证
    /// </summary>
    /// <param name="context"></param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        if ((!request.Headers.TryGetValue(HeaderName, out var token) && !request.Query.TryGetValue(QueryName, out token)))
        {
            context.Result = new JsonResult(new GenericResponse {
                Code = HttpStatusCode.Unauthorized,
                Success = false,
                Message = "Authorization header is missing",
            });
            return;
        }

        if (!Guid.TryParse(token, out var guid))
        {
            context.Result = new JsonResult(new GenericResponse {
                Code = HttpStatusCode.Unauthorized,
                Success = false,
                Message = "Invalid authorization key",
            });
            return;
        }

        var userTokenService = context.HttpContext.RequestServices.GetRequiredService<IUserTokenService>();
        var groupRepository = context.HttpContext.RequestServices.GetRequiredService<GroupRepository>();
        var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<OptionsSetting>>().Value;

        var dbUser = await userTokenService.VerifyToken(guid).ConfigureAwait(false);



        if (dbUser != null && !dbUser.IsBan)
        {
            //如果是配置文件中指定的管理员就覆盖用户组权限
            if (config.Bot.SuperAdmins?.Contains(dbUser.UserID) ?? false)
            {
                dbUser.GroupID = groupRepository.GetMaxGroupId();
            }

            //根据GroupID设置用户权限信息 (封禁用户区别对待)
            var group = groupRepository.GetGroupById(dbUser.GroupID);

            if (group != null)
            {
                dbUser.Right = group.DefaultRight;
                context.HttpContext.Items["Users"] = dbUser;
            }
            else
            {
                context.Result = new JsonResult(new GenericResponse {
                    Code = HttpStatusCode.Unauthorized,
                    Success = false,
                    Message = "User group not found",
                });
                return;
            }
        }
        else
        {
            context.Result = new JsonResult(new GenericResponse {
                Code = HttpStatusCode.Unauthorized,
                Success = false,
                Message = "User not found or user is banned",
            });
            return;
        }
    }
}

