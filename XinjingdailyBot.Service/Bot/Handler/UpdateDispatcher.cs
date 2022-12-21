using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Bot.Handler
{
    public class UpdateDispatcher : IUpdateDispatcher
    {
        private readonly ILogger<UpdateDispatcher> _logger;
        private readonly ITextHelperService _userService;

        public UpdateDispatcher(
            ILogger<UpdateDispatcher> logger,
             ITextHelperService userService
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
