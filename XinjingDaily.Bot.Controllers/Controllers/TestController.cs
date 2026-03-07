using Microsoft.AspNetCore.Mvc;
using XinjingDaily.Bot.Controllers.Controllers.Base;
using XinjingDaily.Bot.IRepository.Redis;

namespace XinjingDaily.Bot.Controllers.Controllers;

[ApiController]
public class TestController(
    IRedisRepository _redisCache) : AbstractController
{
    [HttpGet]
    public async Task<ActionResult> SetString(string key = "test", string value = "123")
    {
        await _redisCache.SetStringAsync(key, value, TimeSpan.FromSeconds(30));
        value = await _redisCache.GetStringAsync(key);
        return Ok(value);
    }

    [HttpGet]
    public async Task<ActionResult> GetString(string key = "test")
    {
        var value = await _redisCache.GetStringAsync(key);
        return Ok(value);
    }
}
