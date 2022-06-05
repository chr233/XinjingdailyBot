using NLog;

namespace XinjingdailyBot.Helpers
{
    internal class LoggerHelper
    {
        internal static Logger Logger { get; private set; } = LogManager.GetLogger(SharedInfo.XJBBot);
    }
}
