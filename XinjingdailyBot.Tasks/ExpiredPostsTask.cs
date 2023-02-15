using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks
{
    /// <summary>
    /// 过期稿件处理
    /// </summary>
    public class ExpiredPostsTask : IHostedService, IDisposable
    {
        private readonly ILogger<ExpiredPostsTask> _logger;
        private readonly IPostService _postService;
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;

        public ExpiredPostsTask(
            ILogger<ExpiredPostsTask> logger,
            IPostService postService,
            IUserService userService,
            ITelegramBotClient botClient,
            IOptions<OptionsSetting> options)
        {
            _logger = logger;
            _postService = postService;
            _userService = userService;
            _botClient = botClient;
            PostExpiredTime = TimeSpan.FromDays(options.Value.Post.PostExpiredTime);
        }

        /// <summary>
        /// 定时器周期
        /// </summary>
        private readonly TimeSpan CheckInterval = TimeSpan.FromDays(3);

        /// <summary>
        /// 稿件过期时间
        /// </summary>
        private TimeSpan PostExpiredTime { get; init; }

        /// <summary>
        /// 计时器
        /// </summary>
        private Timer? _timer = null;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var nextDay = now.AddDays(1).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var tillTomorrow = nextDay - now;

            _timer = new Timer(DoWork, null, tillTomorrow, CheckInterval);

            return Task.CompletedTask;
        }

        private async void DoWork(object? _ = null)
        {
            _logger.LogInformation("开始定时任务, 清理过期稿件任务");

            DateTime expiredDate = DateTime.Now - PostExpiredTime;

            //获取有过期稿件的用户
            var userIDList = await _postService.Queryable()
                .Where(x => (x.Status == PostStatus.Padding || x.Status == PostStatus.Reviewing) && x.ModifyAt < expiredDate)
                .Distinct().Select(x => x.PosterUID).ToListAsync();

            if (!userIDList.Any())
            {
                _logger.LogInformation("结束定时任务, 没有需要清理的过期稿件");
                return;
            }

            _logger.LogInformation("成功获取 {Count} 个有过期稿件的用户", userIDList.Count);

            foreach (var userID in userIDList)
            {
                //获取过期投稿
                var paddingPosts = await _postService.Queryable()
                    .Where(x => x.PosterUID == userID && (x.Status == PostStatus.Padding || x.Status == PostStatus.Reviewing) && x.ModifyAt < expiredDate)
                    .ToListAsync();

                if (!paddingPosts.Any())
                {
                    continue;
                }

                int cTmout = 0, rTmout = 0;
                foreach (var post in paddingPosts)
                {
                    if (post.Status == PostStatus.Padding)
                    {
                        post.Status = PostStatus.ConfirmTimeout;
                        cTmout++;
                    }
                    else
                    {
                        post.Status = PostStatus.ReviewTimeout;
                        rTmout++;
                    }
                    post.ModifyAt = DateTime.Now;

                    await _postService.Updateable(post).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();
                }

                var user = await _userService.Queryable().FirstAsync(x => x.UserID == userID);

                if (user == null)
                {
                    _logger.LogInformation("清理了 {userID} 的 {cTmout} / {rTmout} 条确认/审核超时投稿", userID, cTmout, rTmout);
                }
                else
                {
                    _logger.LogInformation("清理了 {user} 的 {cTmout} / {rTmout} 条确认/审核超时投稿", user.ToString(), cTmout, rTmout);

                    //满足条件则通知投稿人
                    //1.未封禁
                    //2.有PrivateChatID
                    //3.启用通知
                    if (!user.IsBan && user.PrivateChatID > 0 && user.Notification)
                    {
                        StringBuilder sb = new();

                        if (cTmout > 0)
                        {
                            sb.AppendLine($"你有 <code>{cTmout}</code> 份稿件因为确认超时被清理");
                        }

                        if (rTmout > 0)
                        {
                            sb.AppendLine($"你有 <code>{rTmout}</code> 份稿件因为审核超时被清理");
                        }

                        try
                        {
                            await _botClient.SendTextMessageAsync(user.PrivateChatID, sb.ToString(), parseMode: ParseMode.Html, disableNotification: true);
                            await Task.Delay(500);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("通知消息发送失败, 自动禁用更新 {error}", ex);
                            user.PrivateChatID = -1;
                            await Task.Delay(5000);
                        }
                    }

                    user.ExpiredPostCount += rTmout;
                    user.ModifyAt = DateTime.Now;

                    //更新用户表
                    await _userService.Updateable(user).UpdateColumns(x => new { x.PrivateChatID, x.ExpiredPostCount, x.ModifyAt }).ExecuteCommandAsync();
                }
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
