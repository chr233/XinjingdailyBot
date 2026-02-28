using Microsoft.AspNetCore.Mvc;
using XinjingDaily.Bot.Controllers.Controllers.Base;
using XinjingDaily.Bot.IRepository.Redis;

namespace XinjingDaily.Bot.Controllers.Controllers;

[ApiController]
public class TestController(
    IRedisCacheRepository _redisCache) : AbstractController
{
    [HttpGet]
    public async Task<ActionResult> Set(string key = "test", string value = "123")
    {
        await _redisCache.SetAsync(key, value, TimeSpan.FromSeconds(30));
        value = await _redisCache.GetAsync(key);
        return Ok(value);
    }

    [HttpGet]
    public async Task<ActionResult> Get(string key = "test")
    {
        var value = await _redisCache.GetAsync(key);
        return Ok(value);
    }
}
