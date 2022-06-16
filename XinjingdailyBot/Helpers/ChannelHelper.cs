using Telegram.Bot;
using Telegram.Bot.Types;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal sealed class ChannelHelper
    {
        internal static Chat ReviewGroup { get; private set; } = new();
        internal static Chat SubGroup { get; private set; } = new();
        internal static Chat AcceptChannel { get; private set; } = new();
        internal static Chat RejectChannel { get; private set; } = new();
        internal static async Task VerifyChannelConfig(ITelegramBotClient botClient)
        {
            if (long.TryParse(BotConfig.ReviewGroup, out long groupId))
            {
                ReviewGroup = await botClient.GetChatAsync(groupId);
            }
            else
            {
                ReviewGroup = await botClient.GetChatAsync(BotConfig.ReviewGroup);
            }

            if (long.TryParse(BotConfig.SubGroup, out long subGroupId))
            {
                SubGroup = await botClient.GetChatAsync(subGroupId);
            }
            else
            {
                SubGroup = await botClient.GetChatAsync(BotConfig.SubGroup);
            }

            AcceptChannel = await botClient.GetChatAsync(BotConfig.AcceptChannel);
            RejectChannel = await botClient.GetChatAsync(BotConfig.RejectChannel);

            User me = await botClient.GetMeAsync();
            BotName = me.Username ?? "";
        }
    }
}
