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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        public IndexController(ILogger<IndexController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Root
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("启动完成");
        }
    }
}
