namespace XinjingDaily.Bot.Controllers;

/// <summary>
/// HttpContextAccessor扩展
/// </summary>
public static class HttpContextAccessorExtension
{
    /// <summary>
    /// 获取用户
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    //public static UsersLegacy GetUser(this IHttpContextAccessor httpContextAccessor)
    //{
    //    UsersLegacy? user = null;

    //    if (httpContextAccessor.HttpContext?.Items.TryGetValue("Users", out var obj) ?? false)
    //    {
    //        if (obj != null)
    //        {
    //            user = obj as UsersLegacy;
    //        }
    //    }

    //    ArgumentNullException.ThrowIfNull(user);

    //    return user;
    //}
}
