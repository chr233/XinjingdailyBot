using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal static class ChannelHelper
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
        /// <summary>
        /// 发布频道
        /// </summary>
        internal static Chat AcceptChannel { get; private set; } = new();
        /// <summary>
        /// 拒稿存档
        /// </summary>
        internal static Chat RejectChannel { get; private set; } = new();
        /// <summary>
        /// 机器人用户信息
        /// </summary>
        internal static User BotUser { get; private set; } = new();

        /// <summary>
        /// 验证频道设置
        /// </summary>
        /// <param name="botClient"></param>
        /// <returns></returns>
        internal static async Task VerifyChannelConfig(ITelegramBotClient botClient)
        {
            BotUser = await botClient.GetMeAsync();

            Logger.Info($"机器人ID: {BotUser.Id}");
            Logger.Info($"机器人昵称: {BotUser.NickName()}");
            Logger.Info($"机器人用户名: @{BotUser.Username}");

            try
            {
                AcceptChannel = await botClient.GetChatAsync(BotConfig.AcceptChannel);
                Logger.Info($"稿件发布频道: {AcceptChannel.ChatProfile()}");
            }
            catch
            {
                Logger.Error("未找到指定的稿件发布频道, 请检查拼写是否正确");
                throw;
            }

            try
            {

                RejectChannel = await botClient.GetChatAsync(BotConfig.RejectChannel);
                Logger.Info($"拒稿存档频道: {RejectChannel.ChatProfile()}");
            }
            catch
            {
                Logger.Error("未找到指定的拒稿存档频道, 请检查拼写是否正确");
                throw;
            }

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
                Logger.Info($"审核群组: {ReviewGroup.ChatProfile()}");
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
                Logger.Info($"评论区群组: {CommentGroup.ChatProfile()}");
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
                Logger.Info($"频道子群组: {SubGroup.ChatProfile()}");
            }
            catch
            {
                Logger.Error("未找到指定的闲聊群组, 可以使用 /groupinfo 命令获取群组信息");
                SubGroup = new() { Id = -1 };
            }

            if (SubGroup.Id == -1 && CommentGroup.Id != -1)
            {
                SubGroup = CommentGroup;
            }
            else if (CommentGroup.Id == -1 && SubGroup.Id != -1)
            {
                CommentGroup = SubGroup;
            }
        }
    }
}
