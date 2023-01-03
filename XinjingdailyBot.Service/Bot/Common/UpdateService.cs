using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Service.Bot.Common
{
    [AppService(ServiceType = typeof(IUpdateService), ServiceLifetime = LifeTime.Scoped)]
    public class UpdateService : IUpdateService
    {
        private readonly ILogger<UpdateService> _logger;
        private readonly IUserService _userService;
        private readonly IDispatcherService _dispatcherService;

        public UpdateService(
            ILogger<UpdateService> logger,
            IUserService userService,
            IDispatcherService dispatcherService)
        {
            _logger = logger;
            _userService = userService;
            _dispatcherService = dispatcherService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
        {
            _logger.LogUpdate(update);

            var dbUser = await _userService.FetchUserFromUpdate(update);

            if (dbUser == null)
            {
                return;
            }

            var handler = update.Type switch
            {
                UpdateType.ChannelPost => _dispatcherService.OnChannalPostReceived(dbUser, update.ChannelPost!),
                UpdateType.Message => _dispatcherService.OnMessageReceived(dbUser, update.Message!),
                UpdateType.CallbackQuery => _dispatcherService.OnCallbackQueryReceived(dbUser, update.CallbackQuery!),
                //UpdateType.InlineQuery
                //UpdateType.ChosenInlineResult,
                _ => null
            };

            if (handler != null)
            {
                await handler;
            }
        }

        public async Task HandlePollingErrorAsync(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogInformation("处理轮询出错: {ErrorMessage}", ErrorMessage);

            if (exception is RequestException)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}
