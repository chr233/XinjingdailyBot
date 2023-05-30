using Microsoft.AspNetCore.Mvc;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

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
        private readonly IUserTokenService _userTokenService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userTokenService"></param>
        public IndexController(
            ILogger<IndexController> logger,
            IUserTokenService userTokenService)
        {
            _logger = logger;
            _userTokenService = userTokenService;
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

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Users>> TestToken([FromBody] Guid token)
        {
            var user = await _userTokenService.VerifyToken(token);

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
