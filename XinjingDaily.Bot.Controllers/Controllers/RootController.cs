using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XinjingDaily.Bot.Controllers.Controllers.Base;
using XinjingDaily.Bot.Controllers.Responses;
using XinjingDaily.Bot.Infrastructure.Options;

namespace XinjingDaily.Bot.Controllers.Controllers;

[ApiController]
public class RootController(IOptions<SystemConfig> _options) : AbstractController
{
    /// <summary>
    /// 首页
    /// </summary>
    /// <returns></returns>
    [HttpGet("/")]
    public ActionResult<GenericResponse> Index()
    {
        return _options.Value.Swagger ? Redirect("/swagger") : Redirect("/about");
    }

    /// <summary>
    /// 关于
    /// </summary>
    /// <returns></returns>
    [HttpGet("/about")]
    public ActionResult<AboutResponse> GetAbout()
    {
        return Ok(new AboutResponse("机器人启动完成, 请我喝杯快乐水: https://afdian.com/a/chr233"));
    }

    /// <summary>
    /// 错误页
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Error")]
    public ActionResult<GenericResponse> Error()
    {
        var response = GenericResponse.InternalErrorResponse;

        if (_options.Value.Debug)
        {
            var exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            response.Message = exception?.ToString() ?? "null";
        }
        else
        {
            response.Message = "遇到内部错误 打开调试模式获取错误详情";
        }

        return Ok(response);
    }
}
