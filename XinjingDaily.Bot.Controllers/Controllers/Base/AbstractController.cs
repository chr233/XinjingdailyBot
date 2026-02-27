using Microsoft.AspNetCore.Mvc;

namespace XinjingDaily.Bot.Controllers.Controllers.Base;

/// <summary>
/// 基础控制器
/// </summary>
[ApiController]
[Route("/Api/[controller]/[action]")]
[Produces("application/json")]
public abstract class AbstractController : ControllerBase
{
}
