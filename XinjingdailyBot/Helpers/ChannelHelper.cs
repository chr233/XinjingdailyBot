using Telegram.Bot;
using Telegram.Bot.Types;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal sealed class ChannelHelper
    {
        /// <summary>
        /// 审核群组
        /// </summary>
        internal static Chat ReviewGroup { get; private set; } = new();
        /// <summary>
        /// 评论区群组
        /// </summary>
        internal static Chat CommentGroup { get; private set; } = new();
        /// <summary>
        /// 闲聊区群组
        /// </summary>
        internal static Chat SubGroup { get; private set; } = new();
        internal static Chat AcceptChannel { get; private set; } = new();
        internal static Chat RejectChannel { get; private set; } = new();
        internal static async Task VerifyChannelConfig(ITelegramBotClient botClient)
        {
            try
            {
                if (long.TryParse(BotConfig.ReviewGroup, out long groupId))
                {
                    ReviewGroup = await botClient.GetChatAsync(groupId);
                }
                else
                {
                    ReviewGroup = await botClient.GetChatAsync(BotConfig.ReviewGroup);
                }
            }
            catch
            {
                Logger.Error("未找到指定的审核群组, 可以使用 /groupinfo 命令获取群组信息");
                ReviewGroup = new() { Id = -1 };
            }

            try
            {
                if (long.TryParse(BotConfig.CommentGroup, out long subGroupId))
                {
                    CommentGroup = await botClient.GetChatAsync(subGroupId);
                }
                else
                {
                    CommentGroup = await botClient.GetChatAsync(BotConfig.CommentGroup);
                }
            }
            catch
            {
                Logger.Error("未找到指定的评论区群组, 可以使用 /groupinfo 命令获取群组信息");
                CommentGroup = new() { Id = -1 };
            }

            try
            {
                if (long.TryParse(BotConfig.SubGroup, out long subGroupId))
                {
                    SubGroup = await botClient.GetChatAsync(subGroupId);
                }
                else
                {
                    SubGroup = await botClient.GetChatAsync(BotConfig.SubGroup);
                }
            }
            catch
            {
                Logger.Error("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
                SubGroup = new() { Id = -1 };
            }

            AcceptChannel = await botClient.GetChatAsync(BotConfig.AcceptChannel);
            RejectChannel = await botClient.GetChatAsync(BotConfig.RejectChannel);

            User me = await botClient.GetMeAsync();
            BotName = me.Username ?? "";
        }
    }
}
