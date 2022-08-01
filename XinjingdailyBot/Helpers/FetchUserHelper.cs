using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
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
                UpdateType.Message => update.Message!.From,
                UpdateType.EditedMessage => update.EditedMessage!.From,
                UpdateType.CallbackQuery => update.CallbackQuery!.From,
                UpdateType.InlineQuery => update.InlineQuery!.From,
                UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
                _ => null
            };

            Chat? msgChat = update.Type switch
            {
                UpdateType.ChannelPost => update.ChannelPost!.Chat,
                UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
                UpdateType.Message => update.Message!.Chat,
                UpdateType.EditedMessage => update.EditedMessage!.Chat,
                _ => null
            };

            return await FetchDbUser(msgUser, msgChat);
        }

        /// <summary>
        /// 根据MessageUser获取用户
        /// </summary>
        /// <param name="msgUser"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(User? msgUser, Chat? msgChat)
        {
            if (msgUser == null || msgUser.Username == "GroupAnonymousBot")
            {
                return null;
            }

            Users? dbUser = await DB.Queryable<Users>().FirstAsync(x => x.UserID == msgUser.Id);

            long chatID = (msgChat?.Type == ChatType.Private) ? msgChat.Id : -1;

            if (dbUser == null)
            {
                if (!UGroups.TryGetValue(1, out var group))
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
                    IsBot = msgUser.IsBot,
                    IsBan = false,
                    GroupID = group.Id,
                    PrivateChatID = chatID,
                    Right = group.DefaultRight,
                    Level = 1,
                };

                try
                {
                    await DB.Insertable(dbUser).ExecuteCommandAsync();
                    if (IsDebug)
                    {
                        Logger.Debug($"创建用户 {dbUser} 成功");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"创建用户 {dbUser} 失败");
                    Logger.Error(ex);
                    return null;
                }
            }
            else
            {
                bool needUpdate = false;
                //用户名不一致时更新
                if (!(dbUser.UserName.Equals(msgUser.Username ?? "") && dbUser.FirstName.Equals(msgUser.FirstName) && dbUser.LastName.Equals(msgUser.LastName ?? "")))
                {
                    dbUser.UserName = msgUser.Username ?? "";
                    dbUser.FirstName = msgUser.FirstName;
                    dbUser.LastName = msgUser.LastName ?? "";
                    needUpdate = true;
                }

                if (dbUser.IsBot != msgUser.IsBot)
                {
                    dbUser.IsBot = msgUser.IsBot;
                    needUpdate = true;
                }

                if (dbUser.PrivateChatID != chatID)
                {
                    if (chatID != -1)
                    {
                        dbUser.PrivateChatID = chatID;
                        needUpdate = true;
                    }
                }

                //如果被封禁自动覆盖原用户组
                if (dbUser.IsBan)
                {
                    dbUser.GroupID = 6;
                }

                if (!UGroups.ContainsKey(dbUser.GroupID))
                {
                    dbUser.GroupID = 1;
                    needUpdate = true;
                }

                if (BotConfig.SuperAdmins.Contains(dbUser.UserID))
                {
                    dbUser.Right = UserRights.ALL;
                }
                else
                {
                    if (UGroups.TryGetValue(dbUser.GroupID, out var group))
                    {
                        dbUser.Right = group.DefaultRight;
                    }
                    else
                    {
                        return null;
                    }
                }

                if (needUpdate)
                {
                    try
                    {
                        dbUser.ModifyAt = DateTime.Now;
                        await DB.Updateable(dbUser).UpdateColumns(x => new
                        {
                            x.UserName,
                            x.FirstName,
                            x.LastName,
                            x.IsBot,
                            x.GroupID,
                            x.PrivateChatID,
                            x.ModifyAt
                        }).ExecuteCommandAsync();
                        if (IsDebug)
                        {
                            Logger.Debug($"更新用户 {dbUser} 成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"更新用户 {dbUser} 失败");
                        Logger.Error(ex);
                        return null;
                    }
                }
            }
            return dbUser;
        }

        /// <summary>
        /// 根据UserID获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(long? userID)
        {
            if (userID == null)
            {
                return null;
            }
            else
            {
                var dbUser = await DB.Queryable<Users>().FirstAsync(x => x.UserID == userID);
                return dbUser;
            }
        }

        /// <summary>
        /// 根据UserName获取用户
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchDbUser(string? userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }
            else
            {
                var dbUser = await DB.Queryable<Users>().FirstAsync(x => x.UserName == userName);
                return dbUser;
            }
        }

        /// <summary>
        /// 根据ReplyToMessage获取目标用户
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchTargetUser(Message message)
        {
            if (message.ReplyToMessage == null)
            {
                return null;
            }

            Message replyToMsg = message.ReplyToMessage;

            if (replyToMsg.From == null)
            {
                return null;
            }

            //被回复的消息是Bot发的消息
            if (replyToMsg.From.Id == BotUser.Id)
            {
                //在审核群内
                if (message.Chat.Id == ReviewGroup.Id)
                {
                    int msgID = replyToMsg.MessageId;

                    var post = await DB.Queryable<Posts>().FirstAsync(x => x.ReviewMsgID == msgID || x.ManageMsgID == msgID);

                    //判断是不是审核相关消息
                    if (post != null)
                    {
                        //通过稿件读取用户信息
                        return await FetchDbUser(post.PosterUID);
                    }
                }
                //在CMD回调表里查看
                var cmdAction = await DB.Queryable<CmdRecords>().FirstAsync(x => x.MessageID == replyToMsg.MessageId);
                if (cmdAction != null)
                {
                    return await FetchDbUser(cmdAction.UserID);
                }

                return null;
            }

            //获取消息发送人
            return await FetchDbUser(replyToMsg.From.Id);
        }

        /// <summary>
        /// 根据用户输入查找指定用户
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static async Task<Users?> FetchTargetUser(string? target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return null;
            }

            if (target.StartsWith('@'))
            {
                return await FetchDbUser(target.Substring(1));
            }

            Users? dbUser = null;

            if (long.TryParse(target, out var userID))
            {
                dbUser = await FetchDbUser(userID);
            }

            if (dbUser == null)
            {
                dbUser = await FetchDbUser(target);
            }

            return dbUser;
        }
    }
}
