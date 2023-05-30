using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Common;

namespace XinjingdailyBot.Service.Bot.Common
{
    [AppService(typeof(IReceiverService), LifeTime.Scoped)]
    public class ReceiverService : IReceiverService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUpdateService _updateService;
        private readonly ILogger<ReceiverService> _logger;
        private readonly OptionsSetting _optionsSetting;

        public ReceiverService(
            ITelegramBotClient botClient,
            IUpdateService updateService,
            ILogger<ReceiverService> logger,
            IOptions<OptionsSetting> options)
        {
            _botClient = botClient;
            _updateService = updateService;
            _logger = logger;
            _optionsSetting = options.Value;
        }

        public async Task ReceiveAsync(CancellationToken stoppingToken)
        {
            ReceiverOptions receiverOptions = new() {
                AllowedUpdates = Array.Empty<UpdateType>(),
                ThrowPendingUpdates = true || _optionsSetting.Bot.ThrowPendingUpdates,
            };

            _logger.LogInformation("接收服务运行中...");

            await _botClient.ReceiveAsync(
                updateHandler: _updateService,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken);
        }
    }
}
