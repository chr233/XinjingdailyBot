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

            //检查是否封禁, 封禁后仅能使用命令, 不响应其他消息
            if (dbUser.IsBan && !isCommand)
            {
                return;
            }

            bool isMediaGroup = message.MediaGroupId != null;
            bool isPrivateChat = message.Chat.Type == ChatType.Private;
            bool isGroupChat = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;
            bool isCommentGroup = isGroupChat && message.Chat.Id == CommentGroup.Id;
            bool isSubGroup = isGroupChat && message.Chat.Id == SubGroup.Id;
            bool isReviewGroup = isGroupChat && message.Chat.Id == ReviewGroup.Id;
            bool isConfigedGroup = isCommentGroup || isSubGroup || isReviewGroup;

            //尚未设置评论群或者讨论群时始终处理所有群组的消息
            if (CommentGroup.Id == -1 || SubGroup.Id == -1)
            {
                isConfigedGroup = isGroupChat;
            }

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

            if (BotConfig.Debug)
            {
                Logger.LogMessage(message, dbUser);
            }

            switch (message.Type)
            {
                case MessageType.Text when (isConfigedGroup || isPrivateChat) && isCommand:
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

                case MessageType.Text when isConfigedGroup && !dbUser.IsBot:
                    await GroupHandler.HandlerGroupMessage(botClient, dbUser, message);
                    break;

                case MessageType.Photo when !isPrivateChat:
                case MessageType.Audio when !isPrivateChat:
                case MessageType.Video when !isPrivateChat:
                case MessageType.Document when !isPrivateChat:
                case MessageType.Text when !isPrivateChat:
                    if (isGroupChat && !isConfigedGroup && BotConfig.AutoLeaveOtherGroup)
                    {
                        Logger.Warn($"S 自动退出未设置的群组");
                        try
                        {
                            await botClient.LeaveChatAsync(message.Chat.Id);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"S 自动退出群组 {message.Chat.ChatProfile()} 失败 {ex}");
                        }
                    }
                    break;

                default:
                    if (isPrivateChat)
                    {
                        await botClient.AutoReplyAsync("不支持的消息类型, 当前仅支持 文字/图片/视频/音频/文件 投稿", message);
                    }
                    break;
            }
        }
    }
}
