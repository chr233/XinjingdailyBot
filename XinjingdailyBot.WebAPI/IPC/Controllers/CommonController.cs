using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[AllowAnonymous]
[Route("/", Name = "其他")]
public sealed class CommonController : XjbController
{
    private readonly bool _debug;

    /// <summary>
    /// 构造函数
    /// </summary>
    public CommonController(
        IOptions<OptionsSetting> options)
    {
        _debug = options.Value.Debug;
    }

    /// <summary>
    /// 首页
    /// </summary>
    /// <returns></returns>
    [HttpGet("/")]
    public ActionResult<string> Index()
    {
        return Ok("机器人启动完成, 请我喝杯快乐水: https://afdian.net/a/chr233");
    }

    /// <summary>
    /// 错误页
    /// </summary>
    /// <returns></returns>
    [HttpGet("Error")]
    public ActionResult<IExceptionHandlerPathFeature> Error()
    {
        if (_debug)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            return Ok(exceptionHandlerPathFeature);
        }
        else
        {
            return Ok("遇到内部错误 打开调试模式获取错误详情");
        }
    }
}
