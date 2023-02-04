using Microsoft.AspNetCore.Mvc;

namespace XinjingdailyBot.WebAPI.Controllers
{
    /// <summary>
    /// 主页控制器
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly ILogger<IndexController> _logger;

        public IndexController(ILogger<IndexController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("启动完成");
        }
    }
}
