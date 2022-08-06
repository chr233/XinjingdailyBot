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
        private static TimeSpan PostTimeout { get; } = TimeSpan.FromDays(3);
        private static int PostCount { get; } = 50;

        internal static async Task MarkExpiredPost(ITelegramBotClient botClient)
        {
            Logger.Info("T 开始定时任务, 清理过期稿件任务");

            DateTime expiredDate = DateTime.Now - PostTimeout;

            HashSet<long> userIDs = new();

            //标记过期投稿
            Dictionary<long, int> expiredPostDict = new();
            var paddingPosts = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Padding && x.ModifyAt < expiredDate).ToPageListAsync(1, PostCount);

            if (paddingPosts.Any())
            {
                foreach (var post in paddingPosts)
                {
                    post.Status = PostStatus.ConfirmTimeout;
                    post.ModifyAt = DateTime.Now;

                    if (!expiredPostDict.TryAdd(post.PosterUID, 1))
                    {
                        expiredPostDict[post.PosterUID]++;
                    }
                    else
                    {
                        userIDs.Add(post.PosterUID);
                    }
                }
                await DB.Updateable(paddingPosts).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

                Logger.Info($"T 清理了 {paddingPosts.Count} 条确认超时的投稿");
            }

            //标记审核超时的投稿
            Dictionary<long, int> timeoutPostDict = new();
            var timeoutPosts = await DB.Queryable<Posts>().Where(x => x.Status == PostStatus.Reviewing && x.ModifyAt < expiredDate).ToPageListAsync(1, PostCount);

            Logger.Debug($"T 获取 {timeoutPosts.Count} 条审核超时投稿");

            if (timeoutPosts.Any())
            {
                foreach (var post in timeoutPosts)
                {
                    post.Status = PostStatus.ReviewTimeout;
                    post.ModifyAt = DateTime.Now;

                    if (!timeoutPostDict.TryAdd(post.PosterUID, 1))
                    {
                        timeoutPostDict[post.PosterUID]++;
                    }
                    else
                    {
                        userIDs.Add(post.PosterUID);
                    }
                }
                await DB.Updateable(timeoutPosts).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

                Logger.Info($"T 清理了 {timeoutPosts.Count} 条审核超时的投稿");
            }

            //获取用户数据
            var userList = await DB.Queryable<Users>().Where(x => userIDs.Contains(x.UserID)).ToListAsync();

            Logger.Debug($"T 获取 {userList.Count} 条相关用户");

            List<Users> userUpdateList = new();

            //通知投稿人
            StringBuilder sb = new();
            foreach (var user in userList)
            {
                if (expiredPostDict.TryGetValue(user.UserID, out int confirmTimeoutCount))
                {
                    sb.AppendLine($"你有 <code>{confirmTimeoutCount}</code> 份稿件确认超时");
                }
                else
                {
                    confirmTimeoutCount = 0;
                }

                if (timeoutPostDict.TryGetValue(user.UserID, out int revirwTimeoutCount))
                {
                    sb.AppendLine($"你有 <code>{revirwTimeoutCount}</code> 份稿件审核超时");
                }
                else
                {
                    revirwTimeoutCount = 0;
                }

                //满足条件则发送通知
                //1.未封禁
                //2.有PrivateChatID
                //3.启用通知
                //4.消息不为空
                if (!user.IsBan && user.PrivateChatID > 0 && user.Notification && sb.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("可以使用命令 /notificate 开启或关闭此提示");
                    try
                    {
                        await botClient.SendTextMessageAsync(user.PrivateChatID, sb.ToString(), ParseMode.Html, disableNotification: true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"T 通知消息发送失败, 自动禁用更新 {ex.GetType()} {ex.Message}");

                        user.PrivateChatID = -1;

                        await Task.Delay(400);
                    }
                    finally
                    {
                        await Task.Delay(100);
                    }
                }

                user.ExpiredPostCount += revirwTimeoutCount;
                user.ModifyAt = DateTime.Now;
                userUpdateList.Add(user);
            }

            //更新用户表
            if (userUpdateList.Any())
            {
                await DB.Updateable(userUpdateList).UpdateColumns(x => new { x.PrivateChatID, x.ModifyAt }).ExecuteCommandAsync();
                Logger.Info($"T 更新了 {userUpdateList.Count} 个用户");
            }
        }
    }
}
