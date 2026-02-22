using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using StackExchange.Redis;
using XinjingdailyBot.Controllers.Controllers.Base;
using XinjingdailyBot.Service.Common;

namespace XinjingdailyBot.Controllers.Controllers;

[ApiController]
public class TestController(ISqlSugarClient _client, IConnectionMultiplexer _connectionMultiplexer, BotManagerService _botFactoryServices) : AbstractController
{

}
