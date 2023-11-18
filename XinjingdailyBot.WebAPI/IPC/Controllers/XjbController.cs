using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 基础控制器
/// </summary>
[ApiController]
[Route("/Api/[controller]/[action]")]
[Produces("application/json")]
[SwaggerResponse((int)HttpStatusCode.Unauthorized, "未提供有效 Token", typeof(GenericResponse))]
[SwaggerResponse((int)HttpStatusCode.BadRequest, "请求无效", typeof(GenericResponse))]
[SwaggerResponse((int)HttpStatusCode.Forbidden, "无可奉告", typeof(GenericResponse))]
[SwaggerResponse((int)HttpStatusCode.InternalServerError, "内部错误", typeof(GenericResponse))]
[SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "内部错误", typeof(GenericResponse))]
public abstract class XjbController : ControllerBase
{

}
