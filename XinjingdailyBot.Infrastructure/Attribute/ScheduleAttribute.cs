namespace XinjingdailyBot.Infrastructure.Attribute;

/// <summary>
/// 标记定时任务
/// </summary>
/// <remarks>
/// 标记定时任务
/// </remarks>
/// <param name="schedule">Cron表达式</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ScheduleAttribute(string schedule) : System.Attribute
{
    /// <summary>
    /// Cron表达式
    /// </summary>
    public string Schedule { get; set; } = schedule;
}
