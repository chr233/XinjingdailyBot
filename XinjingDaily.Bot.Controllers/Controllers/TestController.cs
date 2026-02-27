using Microsoft.AspNetCore.Mvc;
using XinjingDaily.Bot.Controllers.Controllers.Base;
using XinjingDaily.Bot.IRepository.Redis;
using XinjingDaily.Bot.Service.Common;

namespace XinjingDaily.Bot.Controllers.Controllers;

[ApiController]
public class TestController(
    IRedisCacheRepository _redisCache,
    BotManagerService _botFactoryServices) : AbstractController
{
    [HttpGet]
    public async Task<ActionResult> Set(string key = "test", string value = "123")
    {
        await _redisCache.SetAsync(key, value, TimeSpan.FromSeconds(30));
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult> Get(string key = "test")
    {
        var value = await _redisCache.GetAsync(key);
        return Ok(value);
    }
}
