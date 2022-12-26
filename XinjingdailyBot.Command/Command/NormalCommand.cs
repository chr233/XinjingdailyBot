using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class NormalCommand
    {
        private readonly ILogger<NormalCommand> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;

        public NormalCommand(
            ILogger<NormalCommand> logger,
            ITelegramBotClient botClient,
            IUserService userService)
        {
            _logger = logger;
            _botClient = botClient;
            _userService = userService;
        }

        [TextCmd("ECHO", Alias = "E")]
        public async Task Echo(Users users, Message message)
        {
            await _botClient.SendCommandReply("test", message);
        }


        [TextCmd("TEST")]
        public async Task Test(Users users, Message message)
        {
            await _botClient.SendCommandReply("test", message);
        }
    }
}
