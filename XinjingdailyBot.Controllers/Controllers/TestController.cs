using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using StackExchange.Redis;
using Telegram.Bot;
using XinjingdailyBot.Controllers.Controllers.Base;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Common;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.Controllers.Controllers;

[ApiController]
public class TestController(ISqlSugarClient _client, IConnectionMultiplexer _connectionMultiplexer, BotManagerService _botFactoryServices) : AbstractController
{

}
