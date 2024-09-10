using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.Controllers.Controllers;

[ApiController]
public class TestController(ISqlSugarClient client) : ControllerBase
{
    /// <summary>
    /// 首页
    /// </summary>
    /// <returns></returns>
    [HttpPost("/Test")]
    public async Task<ActionResult<GenericResponse>> Index()
    {
        for (int i = 0; i < 100; i++)
        {
            var dig = new Dialogues {
                ChatID = i * 100,
                MessageID = Random.Shared.NextInt64(),
                CreateAt = DateTime.Now,
            };
            await client.Insertable(dig).SplitTable().ExecuteReturnSnowflakeIdAsync();
        }

        return Ok();
    }

    [HttpGet("/Test")]
    public async Task<ActionResult<GenericResponse>> Test(long chatId)
    {
        var list = client.Queryable<Dialogues>()
         .Where(it => it.Id > 1) //适合有索引列，单条或者少量数据查询
         .SplitTable().ToList();//没有条件就是全部表

        return Ok(list);
    }
}
