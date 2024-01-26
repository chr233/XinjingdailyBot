using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
public sealed class CommonControllerCommonController(
        IOptions<OptionsSetting> _options) : XjbController
{
    /// <summary>
    /// 首页
    /// </summary>
    /// <returns></returns>
    [HttpGet("/")]
    public ActionResult<GenericResponse> Index()
    {
        return Ok(new GenericResponse {
            Code = HttpStatusCode.OK,
            Success = true,
            Message = "机器人启动完成, 请我喝杯快乐水: https://afdian.net/a/chr233"
        });
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
