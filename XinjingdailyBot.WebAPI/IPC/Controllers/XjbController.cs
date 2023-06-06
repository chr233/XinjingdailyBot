using Microsoft.AspNetCore.Mvc;

namespace XinjingdailyBot.WebAPI.IPC.Controllers
{
    /// <summary>
    /// 基础控制器
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("Api")]
    public abstract class XjbController : ControllerBase    {    }
}
