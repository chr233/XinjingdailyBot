using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[AllowAnonymous]
[Route("Api", Name = "首页")]
public sealed class IndexController : XjbController
{
    private readonly IUserTokenService _userTokenService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userTokenService"></param>
    public IndexController(
        IUserTokenService userTokenService)
    {
        _userTokenService = userTokenService;
    }

    /// <summary>
    /// 测试Token有效
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    public async Task<ActionResult<bool>> TestToken(Guid token)
    {
        var user = await _userTokenService.VerifyToken(token);
        if (user != null)
        {
            return Ok(true);
        }
        else
        {
            return Ok(false);
        }
    }
}
