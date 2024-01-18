namespace XinjingdailyBot.Infrastructure.Attribute;

/// <summary>
/// 标记定时任务
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JobAttribute : System.Attribute
{
    /// <summary>
    /// Cron表达式
    /// </summary>
    public string Schedule { get; set; }

    /// <summary>
    /// 标记定时任务
    /// </summary>
    /// <param name="schedule">Cron表达式</param>
    public JobAttribute(string schedule)
    {
        Schedule = schedule;
    }
}
