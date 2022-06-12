using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers
{
    internal static class FetchUserHelper
    {
        /// <summary>
        /// 根据Update获取发送消息的用户
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(Update update)
        {
            User? msgUser = update.Type switch
            {
                UpdateType.ChannelPost => update.ChannelPost!.From,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.From,
                UpdateType.ShippingQuery => update.ShippingQuery!.From,
                UpdateType.PreCheckoutQuery => update.PreCheckoutQuery!.From,
                UpdateType.Message => update.Message!.From,
                UpdateType.EditedMessage => update.EditedMessage!.From,
                UpdateType.CallbackQuery => update.CallbackQuery!.From,
                UpdateType.InlineQuery => update.InlineQuery!.From,
                UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
                _ => null
            };

            return await FetchDbUser(msgUser);
        }

        /// <summary>
        /// 根据MessageUser获取用户
        /// </summary>
        /// <param name="msgUser"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(User? msgUser)
        {
            if (msgUser == null || msgUser.IsBot)
            {
                return null;
            }

            Users? dbUser = await DB.Queryable<Users>().FirstAsync(x => x.UserID == msgUser.Id);

            if (dbUser == null)
            {
                if (!UGroups.TryGetValue(1, out Groups? group))
                {
                    Logger.Error("不存在 Id 为 1 的权限组, 请重建数据库");
                    return null;
                }

                dbUser = new()
                {
                    UserID = msgUser.Id,
                    UserName = msgUser.Username ?? "",
                    FirstName = msgUser.FirstName,
                    LastName = msgUser.LastName ?? "",
                    GroupID = group.Id,
                    Right = group.DefaultRight,
                    Level = 0,
                };

                try
                {
                    await DB.Insertable(dbUser).ExecuteCommandAsync();
                    Logger.Debug($"创建用户 {dbUser} 成功");
                }
                catch (Exception ex)
                {
                    Logger.Debug($"创建用户 {dbUser} 失败");
                    Logger.Error(ex);
                    return null;
                }
            }
            else if (
                !(dbUser.UserName.Equals(msgUser.Username ?? "")
                && dbUser.FirstName.Equals(msgUser.FirstName)
                && dbUser.LastName.Equals(msgUser.LastName ?? "")))
            {
                dbUser.UserName = msgUser.Username ?? "";
                dbUser.FirstName = msgUser.FirstName;
                dbUser.LastName = msgUser.LastName ?? "";
                dbUser.ModifyAt = DateTime.Now;

                try
                {
                    await DB.Updateable(dbUser).UpdateColumns(x => new { x.UserName, x.FirstName, x.LastName, x.ModifyAt }).ExecuteCommandAsync();
                    Logger.Debug($"更新用户 {dbUser} 成功");
                }
                catch (Exception ex)
                {
                    Logger.Debug($"更新用户 {dbUser} 失败");
                    Logger.Error(ex);
                    return null;
                }
            }
            return dbUser;
        }

        /// <summary>
        /// 根据Message获取用户, 可以获取被回复的用户
        /// </summary>
        /// <param name="msgUser"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(Message message)
        {
            Message? replyMsg = message.ReplyToMessage;

            if (replyMsg != null)
            {
                if (replyMsg.Chat.Id == ReviewGroup.Id && replyMsg.From!.Id == BotID) // 当前会话为审核频道, 并且发布者为当前bot
                {
                    int mid = replyMsg.MessageId;
                    Posts? post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == mid || x.ManageMsgID == mid);
                    if (post != null)
                    {
                        Users? dbUser = await DB.Queryable<Users>().FirstAsync(x => x.UserID == post.PosterUID);
                        return dbUser;
                    }
                    return null;
                }
                else
                {
                    return await FetchDbUser(replyMsg.From);
                }
            }
            else
            {
                return await FetchDbUser(message.From);
            }
        }
    }
}