using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks
{
    /// <summary>
    /// 发布广告
    /// </summary>
    public class PostAdvertiseTask : IHostedService, IDisposable
    {
        private readonly ILogger<PostAdvertiseTask> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITelegramBotClient _botClient;
        private readonly IAdvertisesService _advertisesService;

        public PostAdvertiseTask(
            ILogger<PostAdvertiseTask> logger,
            IServiceProvider serviceProvider,
            ITelegramBotClient botClient,
            IAdvertisesService advertisesService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _botClient = botClient;
            _advertisesService = advertisesService;
        }

        /// <summary>
        /// 发布频道置顶间隔
        /// </summary>
        private readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 计时器
        /// </summary>
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextDay = now.AddDays(1).AddHours(19 - now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var tillTomorrow = nextDay - now;

#if DEBUG
            tillTomorrow = TimeSpan.FromSeconds(5);
#endif

            _timer = new Timer(DoWork, null, tillTomorrow, CheckInterval);

            return Task.CompletedTask;
        }

        private async void DoWork(object? _ = null)
        {
            _logger.LogInformation("开始定时任务, 发布广告");

            var ad = await _advertisesService.GetPostableAdvertise();

            if (ad == null)
            {
                return;
            }

            var channelService = _serviceProvider.GetService<IChannelService>();

            if (channelService == null)
            {
                _logger.LogError("获取服务 {type} 失败", nameof(IChannelService));
                return;
            }

            var operates = new List<(AdMode, ChatId)>
            {
               new (AdMode.AcceptChannel, channelService.AcceptChannel.Id),
               new (AdMode.RejectChannel, channelService.RejectChannel.Id),
               new (AdMode.ReviewGroup, channelService.ReviewGroup.Id),
               new (AdMode.CommentGroup, channelService.CommentGroup.Id),
               new (AdMode.SubGroup, channelService.SubGroup.Id),
            };

            foreach (var (mode, chatId) in operates)
            {
                if (ad.Mode.HasFlag(mode) && chatId.Identifier != 0)
                {
                    try
                    {
                        var msgId = await _botClient.CopyMessageAsync(chatId, ad.ChatID, (int)ad.MessageID, disableNotification: true);
                        ad.ShowCount++;
                        if (ad.PinMessage)
                        {
                            await _botClient.PinChatMessageAsync(chatId, msgId.Id, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("投放广告出错: {error}", ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(500);
                    }
                }
                await _advertisesService.UpdateAsync(ad);
            }
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
