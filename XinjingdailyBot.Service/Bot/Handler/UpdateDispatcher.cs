using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public class UpdateDispatcher : IUpdateDispatcher
    {
        private readonly ILogger<UpdateDispatcher> _logger;
        private readonly IUserService _userService;

        public UpdateDispatcher(
            ILogger<UpdateDispatcher> logger,
             IUserService userService
            )
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

        }
    }
}
