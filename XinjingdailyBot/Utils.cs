using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Storage;

namespace XinjingdailyBot
{
    internal static class Utils
    {
        internal static long BotID { get; set; }
        internal static Config BotConfig => ConfigHelper.BotConfig;

        internal static NLog.Logger Logger => LoggerHelper.Logger;

        internal static SqlSugarScope DB => DataBaseHelper.DB;

        internal static Dictionary<int, Models.Groups> UGroups => DataBaseHelper.UGroups;
        internal static Dictionary<int, Models.Levels> ULevels => DataBaseHelper.ULevels;

        internal static string BotName { get; set; } = "";
        internal static Chat ReviewGroup => ChannelHelper.ReviewGroup;
        internal static Chat SubGroup => ChannelHelper.SubGroup;
        internal static Chat AcceptChannel => ChannelHelper.AcceptChannel;
        internal static Chat RejectChannel => ChannelHelper.RejectChannel;

        internal static Version MyVersion => typeof(Program).Assembly.GetName().Version ?? new Version("0");
    }
}
