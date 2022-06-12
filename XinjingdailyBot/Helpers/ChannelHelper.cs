using Telegram.Bot;
using Telegram.Bot.Types;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal sealed class ChannelHelper
    {
        internal static Chat ReviewGroup = new();
        internal static Chat AcceptChannel = new();
        internal static Chat RejectChannel = new();
        internal static async Task VerifyChannelConfig(ITelegramBotClient botClient)
        {
            ReviewGroup = await botClient.GetChatAsync(BotConfig.ReviewGroup);
            AcceptChannel = await botClient.GetChatAsync(BotConfig.AcceptChannel);
            RejectChannel = await botClient.GetChatAsync(BotConfig.RejectChannel);
        }
    }
}
