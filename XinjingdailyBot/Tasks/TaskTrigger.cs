namespace XinjingdailyBot.Tasks
{
    internal static class TaskTrigger
    {
        //TODO

        //private static Dictionary<string, Task> TaskList = new();
        //private static Dictionary<string, TimeSpan> TaskInterval = new();
        //private static Dictionary<string, DateTime> TaskNextExec = new();

        private static TimeSpan TaskPeriod { get; }

        private static DateTime LastExec { get; set; }

        static TaskTrigger()
        {
            TaskPeriod = TimeSpan.FromDays(3);
            LastExec = DateTime.MinValue;
        }

        internal static async Task HandleTick(ITelegramBotClient botClient)
        {
            DateTime now = DateTime.Now;
            if (now - LastExec >= TaskPeriod)
            {
                LastExec = now;
                await ExpiredPostsTask.MarkExpiredPost(botClient);
            }
        }
    }
}
