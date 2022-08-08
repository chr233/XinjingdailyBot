using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Tasks
{
    internal static class ExpiredPostsTask
    {
        /// <summary>
        /// 超时时间设定
        /// </summary>
        private static TimeSpan PostTimeout { get; } = TimeSpan.FromDays(3);

        internal static async Task MarkExpiredPost(ITelegramBotClient botClient)
        {
            Logger.Info("T 开始定时任务, 清理过期稿件任务");

            DateTime expiredDate = DateTime.Now - PostTimeout;

            //获取有过期稿件的用户
            var userIDList = await DB.Queryable<Posts>()
                .Where(x => (x.Status == PostStatus.Padding || x.Status == PostStatus.Reviewing) && x.ModifyAt < expiredDate)
                .Distinct().Select(x => x.PosterUID).ToListAsync();

            if (!userIDList.Any())
            {
                Logger.Info("T 结束定时任务, 没有需要清理的过期稿件");
                return;
            }

            Logger.Info($"T 成功获取 {userIDList.Count} 个有过期稿件的用户");

            foreach (var userID in userIDList)
            {
                //获取过期投稿
                var paddingPosts = await DB.Queryable<Posts>()
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
                }

                await DB.Updateable(paddingPosts).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

                var user = await DB.Queryable<Users>().FirstAsync(x => x.UserID == userID);

                if (user == null)
                {
                    Logger.Info($"T 清理了 {userID} 的 {cTmout} / {rTmout} 条确认/审核超时投稿");
                }
                else
                {
                    Logger.Info($"T 清理了 {user} 的 {cTmout} / {rTmout} 条确认/审核超时投稿");

                    //满足条件则通知投稿人
                    //1.未封禁
                    //2.有PrivateChatID
                    //3.启用通知
                    if (!user.IsBan && user.PrivateChatID > 0 && user.Notification)
                    {
                        StringBuilder sb = new();

                        if (cTmout > 0)
                        {
                            sb.AppendLine($"你有 <code>{cTmout}</code> 份稿件确认超时");
                        }

                        if (rTmout > 0)
                        {
                            sb.AppendLine($"你有 <code>{rTmout}</code> 份稿件审核超时");
                        }
                        sb.AppendLine();
                        sb.AppendLine("可以使用命令 /notification 开启或关闭此提示");

                        try
                        {
                            await botClient.SendTextMessageAsync(user.PrivateChatID, sb.ToString(), ParseMode.Html, disableNotification: true);
                            await Task.Delay(100);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"T 通知消息发送失败, 自动禁用更新 {ex.GetType()} {ex.Message}");
                            user.PrivateChatID = -1;
                            await Task.Delay(500);
                        }
                    }

                    user.ExpiredPostCount += rTmout;
                    user.ModifyAt = DateTime.Now;

                    //更新用户表
                    await DB.Updateable(user).UpdateColumns(x => new { x.PrivateChatID, x.ExpiredPostCount, x.ModifyAt }).ExecuteCommandAsync();
                }
            }
        }
    }
}
