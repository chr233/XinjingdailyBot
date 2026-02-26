using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using StackExchange.Redis;
using XinjingDaily.Bot.Controllers.Controllers.Base;
using XinjingDaily.Bot.Service.Common;

namespace XinjingDaily.Bot.Controllers.Controllers;

[ApiController]
public class TestController(ISqlSugarClient _client, IConnectionMultiplexer _connectionMultiplexer, BotManagerService _botFactoryServices) : AbstractController
{

}
