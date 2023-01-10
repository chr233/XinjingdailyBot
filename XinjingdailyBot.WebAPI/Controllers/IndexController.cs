using Microsoft.AspNetCore.Mvc;

namespace XinjingdailyBot.WebAPI.Controllers
{
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
            return Ok("Æô¶¯Íê³É");
        }
    }
}