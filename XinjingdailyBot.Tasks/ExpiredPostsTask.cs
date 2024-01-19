using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 过期稿件处理
/// </summary>
[Job("0 0 0 * * ?")]
public sealed class ExpiredPostsTask(
        ILogger<ExpiredPostsTask> _logger,
        IPostService _postService,
        IUserService _userService,
        ITelegramBotClient _botClient,
        IOptions<OptionsSetting> _options) : IJob
{

    /// <summary>
    /// 稿件过期时间
    /// </summary>
    private readonly TimeSpan PostExpiredTime = _options.Value.Post.PostExpiredTime > 0 ? TimeSpan.FromDays(_options.Value.Post.PostExpiredTime) : TimeSpan.Zero;

    public async Task Execute(IJobExecutionContext context)
    {
        if (PostExpiredTime.TotalDays == 0)
        {
            return;
        }

        _logger.LogInformation("开始定时任务, 清理过期稿件任务");

        var expiredDate = DateTime.Now - PostExpiredTime;

        //获取有过期稿件的用户
        var expiredPosts = await _postService.GetExpiredPosts(expiredDate);
        var userIDList = expiredPosts.Select(x => x.PosterUID).Distinct().ToList();

        if (userIDList.Count == 0)
        {
            _logger.LogInformation("结束定时任务, 没有需要清理的过期稿件");
            return;
        }

        _logger.LogInformation("成功获取 {Count} 个有过期稿件的用户", userIDList.Count);

        foreach (var userID in userIDList)
        {
            //获取过期投稿
            var paddingPosts = await _postService.GetExpiredPosts(userID, expiredDate);

            if (paddingPosts.Count == 0)
            {
                continue;
            }

            int cTmout = 0, rTmout = 0;
            foreach (var post in paddingPosts)
            {
                EPostStatus status;
                if (post.Status == EPostStatus.Padding)
                {
                    status = EPostStatus.ConfirmTimeout;
                    cTmout++;
                }
                else
                {
                    status = EPostStatus.ReviewTimeout;
                    rTmout++;
                }
                post.ModifyAt = DateTime.Now;

                await _postService.UpdatePostStatus(post, status);
            }

            var user = await _userService.FetchUserByUserID(userID);

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
                    var sb = new StringBuilder();

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
                        _logger.LogError(ex, "通知消息发送失败, 自动禁用更新");
                        user.PrivateChatID = -1;
                        await Task.Delay(5000);
                    }
                }

                user.ExpiredPostCount += rTmout;

                //更新用户表
                await _userService.UpdateUserPostCount(user);
            }
        }
    }
}
