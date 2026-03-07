namespace XinjingDaily.Bot.Infrastructure.Attribute;

/// <summary>
/// 标记定时任务
/// </summary>
/// <param name="schedule">Cron表达式</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScheduleAttribute : System.Attribute
{
    /// <summary>
    /// Cron表达式
    /// </summary>
    public string Schedule { get; init; }

    /// <remarks>
    /// 标记定时任务
    /// </remarks>
    public ScheduleAttribute(string schedule)
    {
        Schedule = schedule;
    }
}
