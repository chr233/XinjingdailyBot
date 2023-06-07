using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.WebAPI.IPC.Data;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[Route("Api/[controller]", Name = "投稿")]
[Authorize]
public sealed class PostController : XjbController
{
    private readonly ILogger<PostController> _logger;
    private readonly IUserTokenService _userTokenService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="userTokenService"></param>
    public PostController(
        ILogger<PostController> logger,
        IUserTokenService userTokenService)
    {
        _logger = logger;
        _userTokenService = userTokenService;
    }

    /// <summary>
    /// 创建稿件
    /// </summary>
    /// <param name="token"></param>
    /// <param name="post"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    public async Task<ActionResult<Users>> CreatePost(Guid token, [FromForm] CreatePostData post)
    {
        var user = await _userTokenService.VerifyToken(token);

        if (true)
        {
            _logger.LogInformation("{post}", post);
        }

        if (user != null)
        {
            return Ok(user);
        }
        else
        {
            return NotFound();
        }
    }
}
