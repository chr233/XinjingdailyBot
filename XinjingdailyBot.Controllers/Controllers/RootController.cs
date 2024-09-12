using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using XinjingdailyBot.Controllers.Controllers.Base;
using XinjingdailyBot.Controllers.Responses;
using XinjingdailyBot.Infrastructure.Options;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.Controllers.Controllers;

[ApiController]
public class RootController(IOptions<ApiConfig> _options) : AbstractController
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

    private readonly AboutResponse about = new AboutResponse("机器人启动完成, 请我喝杯快乐水: https://afdian.com/a/chr233");

    [HttpGet("/about")]
    public ActionResult<AboutResponse> GetAbout()
    {
        return Ok(about);
    }

    /// <summary>
    /// 错误页
    /// </summary>
    /// <returns></returns>
    [HttpGet("/Error")]
    public ActionResult<GenericResponse<string>> Error()
    {
        var response = new GenericResponse<IExceptionHandlerPathFeature> {
            Code = HttpStatusCode.InternalServerError,
            Success = false,
        };

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
