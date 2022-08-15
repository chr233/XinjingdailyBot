using XinjingdailyBot.Tasks;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal static class TaskHelper
    {
        /// <summary>
        /// 定时任务执行器
        /// </summary>
        internal static Timer? TaskTimer { get; private set; }

        internal static void InitTasks(ITelegramBotClient botClient)
        {
            TaskTimer = new(
                async (object? state) =>
                {
                    try
                    {
                        await TaskTrigger.HandleTick(botClient);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"T 执行定时任务出错 {ex}");
                    }
                },
                null,
                TimeSpan.FromDays(0),
                TimeSpan.FromDays(1)
            );
        }
    }
}
