using Microsoft.AspNetCore.Mvc;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 基础控制器
/// </summary>
[ApiController]
[Route("/Api/[controller]/[action]")]
[Produces("application/json")]
public abstract class XjbController : ControllerBase
{
}
