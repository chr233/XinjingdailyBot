using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;

namespace XinjingdailyBot.Tasks
{
    public class RejectChannelTask : IHostedService, IDisposable
    {
        private readonly ILogger<RejectChannelTask> _logger;
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;


        public RejectChannelTask(
            ILogger<RejectChannelTask> logger,
            IChannelService channelService,
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _channelService = channelService;
            _botClient = botClient;
        }

        /// <summary>
        /// 发布频道置顶间隔
        /// </summary>
        private readonly TimeSpan PostNoticePeriod = TimeSpan.FromDays(1);

        /// <summary>
        /// 计时器
        /// </summary>
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextDay = now.AddDays(1).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var tillTomorrow = nextDay - now;

            _timer = new Timer(DoWork, null, tillTomorrow, PostNoticePeriod);

            return Task.CompletedTask;
        }

        private async void DoWork(object? _ = null)
        {
            _logger.LogInformation("开始定时任务, 置顶拒稿频道通知");

            var acceptChannel = _channelService.AcceptChannel;

            string descText = string.Format("此频道为 {0}({1}) 的附属频道\r\n此频道仅用于存档未通过的投稿, 频道中的内容均来自用户投稿\r\n本频道中的一切内容不代表 {0} 的立场", acceptChannel.Title, acceptChannel.ChatID());

            var rejectChannel = _channelService.RejectChannel;
            var message = await _botClient.SendTextMessageAsync(rejectChannel, descText);
            await _botClient.PinChatMessageAsync(rejectChannel, message.MessageId);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
