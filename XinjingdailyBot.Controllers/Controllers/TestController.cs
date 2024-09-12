using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using StackExchange.Redis;
using Telegram.Bot;
using XinjingdailyBot.Controllers.Controllers.Base;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.Controllers.Controllers;

[ApiController]
public class TestController(ISqlSugarClient _client, IConnectionMultiplexer _connectionMultiplexer, BotManagerService _botFactoryServices) : AbstractController
{
    /// <summary>
    /// 首页
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<GenericResponse>> Test1()
    {
        for (int i = 0; i < 10; i++)
        {
            var dig = new Dialogues {
                ChatID = i * 100,
                MessageID = Random.Shared.NextInt64(),
                CreateAt = DateTime.Now,
            };
            await _client.Insertable(dig).SplitTable().ExecuteReturnSnowflakeIdAsync();
        }

        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse>> Test2()
    {
        var list = await _client.Queryable<Dialogues>()
         .Where(it => it.Id > 1) //适合有索引列，单条或者少量数据查询
         .SplitTable(c => c.Take(10)).ToPageListAsync(0, 10);//没有条件就是全部表

        return Ok(list);
    }


    [HttpGet]
    public async Task<ActionResult<GenericResponse>> Test3()
    {
        string key = "exampleKey";
        TimeSpan cacheDuration = TimeSpan.FromMinutes(5);

        // 获取数据，优先从缓存中获取，如果缓存过期则从数据库中获取并回写缓存
        string data = await GetOrSetCacheAsync(key, GetDataFromDbAsync, cacheDuration);

        return Ok(data);
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse>> Test4()
    {
        var bots = _botFactoryServices.GetBots;

        var tasks = bots.Select(x => x.SendTextMessageAsync(556482500, "text"));

        await Task.WhenAll(tasks);

        return Ok(bots);
    }

    private IDatabase _db = _connectionMultiplexer.GetDatabase();

    private async Task<string> GetOrSetCacheAsync(string key, Func<Task<string>> getDataFromDb, TimeSpan cacheDuration)
    {
        // 尝试从缓存中获取数据
        string cachedData = await _db.StringGetAsync(key);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return cachedData;
        }

        // 如果缓存中没有数据，从数据库中获取
        string dataFromDb = await getDataFromDb();

        // 将数据存入缓存，并设置过期时间
        await _db.StringSetAsync(key, dataFromDb, cacheDuration);

        return dataFromDb;
    }

    private async Task<string> GetDataFromDbAsync()
    {
        // 模拟从数据库中获取数据
        await Task.Delay(5000); // 模拟数据库查询延迟
        return "Data from database";
    }
}
