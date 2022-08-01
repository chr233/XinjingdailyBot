using NLog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Helpers
{
    internal static class LoggerHelper
    {
        internal static Logger Logger { get; private set; } = LogManager.GetLogger(SharedInfo.XJBBot);

        internal static void LogUpdate(this Logger logger, Update update, Users dbUser)
        {

        }

        internal static void LogMessage(this Logger logger, Message message, Users dbUser)
        {

        }

        internal static void LogCallbackQuery(this Logger logger, CallbackQuery query, Users dbUser)
        {

        }
    }
}
