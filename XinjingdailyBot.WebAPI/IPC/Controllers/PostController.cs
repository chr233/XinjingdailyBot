using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.WebAPI.IPC.Controllers
{
    /// <summary>
    /// 主页控制器
    /// </summary>
    [ApiController]
    [Route("Api/[controller]")]
    public class PostController : XjbController
    {
        private readonly ILogger<PostController> _logger;
        private readonly IUserTokenService _userTokenService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userTokenService"></param>
        public PostController(
            ILogger<PostController> logger,
            IUserTokenService userTokenService)
        {
            _logger = logger;
            _userTokenService = userTokenService;
        }

        public sealed record Test
        {
            public IFormFileCollection? Media { get; set; }
            public string? Text { get; set; }
            public string? From { get; set; }
        }

        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="token"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<Users>> TestToken(Guid token, [FromForm] Test test)
        {
            var user = await _userTokenService.VerifyToken(token);

            if (false)
            {

            }

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
