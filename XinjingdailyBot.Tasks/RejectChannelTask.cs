using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks
{
    public class RejectChannelTask : IHostedService, IDisposable
    {
        private readonly ILogger<RejectChannelTask> _logger;
        private readonly IChannelService _channelService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
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

            long chatId = _channelService.RejectChannel.Id;

            var message = await _botClient.SendTextMessageAsync(chatId, "12345");
            await _botClient.PinChatMessageAsync(chatId, message.MessageId);
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
