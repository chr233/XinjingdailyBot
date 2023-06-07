using Microsoft.AspNetCore.Mvc;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.WebAPI.IPC.Requests;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[Route("Api/[controller]", Name = "投稿")]
public sealed class PostController : XjbController
{
    private readonly ILogger<PostController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpContextAccessor"></param>
    public PostController(
        ILogger<PostController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 创建稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    public async Task<ActionResult<GenericResponse<NewPosts>>> CreatePost([FromForm] CreatePostRequest post)
    {
        var user = _httpContextAccessor.GetUser();

        await Task.Delay(200);

        _logger.LogInformation("{post}", post);

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
