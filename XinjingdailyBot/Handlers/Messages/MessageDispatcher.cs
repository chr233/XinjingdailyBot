using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Localization;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages
{
    internal static class MessageDispatcher
    {
        internal static string NSFWWrning { get; } = $"{Emojis.Warning} NSFW 提前预警 {Emojis.Warning}";

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task BotOnMessageReceived(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            MessageType msgType = message.Type;
            string msgText = msgType == MessageType.Text ? message.Text! : "";

            bool isCommand = msgText.StartsWith('/');
            bool isMediaGroup = message.MediaGroupId != null;
            bool isPrivateChat = message.Chat.Type == ChatType.Private;
            bool isGroupChat = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;
            bool isCommentGroup = isGroupChat && message.Chat.Id == CommentGroup.Id;
            bool isSubGroup = isGroupChat && message.Chat.Id == SubGroup.Id;

            //取消绑定子频道的消息置顶
            if (dbUser.UserID == 777000)//Telegram
            {
                if (isSubGroup || isCommentGroup)
                {
                    try
                    {
                        if (NSFWWrning == message.Text)
                        {
                            await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                        else
                        {
                            await botClient.UnpinChatMessageAsync(message.Chat.Id, message.MessageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                return;
            }

            if (BotConfig.Debug && !isCommand)
            {
                Logger.Debug($"M {message.Type} {dbUser} {msgText}");
            }

            //检查是否封禁, 封禁后仅能使用命令, 不响应其他消息
            if (dbUser.IsBan && !isCommand)
            {
                return;
            }

            switch (message.Type)
            {
                case MessageType.Text when isCommand:
                    await CommandHandler.HandleCommand(botClient, dbUser, message);
                    break;

                case MessageType.Text when isPrivateChat:
                    await PostHandler.HandleTextPosts(botClient, dbUser, message);
                    break;
                case MessageType.Photo when isMediaGroup && isPrivateChat:
                case MessageType.Audio when isMediaGroup && isPrivateChat:
                case MessageType.Video when isMediaGroup && isPrivateChat:
                case MessageType.Document when isMediaGroup && isPrivateChat:
                    await PostHandler.HandleMediaGroupPosts(botClient, dbUser, message);
                    break;

                case MessageType.Photo when isPrivateChat:
                case MessageType.Audio when isPrivateChat:
                case MessageType.Video when isPrivateChat:
                case MessageType.Document when isPrivateChat:
                    await PostHandler.HandleMediaPosts(botClient, dbUser, message);
                    break;

                case MessageType.Text when (isCommentGroup || isSubGroup) && !dbUser.IsBot:
                    await GroupHandler.HandlerGroupMessage(botClient, dbUser, message);
                    break;

                default:
                    if (isPrivateChat)
                    {
                        await botClient.AutoReplyAsync("未处理的消息", message);
                    }
                    break;
            }
        }
    }
}
