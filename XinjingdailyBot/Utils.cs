using XinjingdailyBot.Storage;
using XinjingdailyBot.Helpers;
using SqlSugar;

namespace XinjingdailyBot
{
    internal static class Utils
    {
        internal static Config BotConfig => ConfigHelper.BotConfig;

        internal static NLog.Logger Logger => LoggerHelper.Logger;

        internal static SqlSugarScope DB => DataBaseHelper.DB;
    }
}
