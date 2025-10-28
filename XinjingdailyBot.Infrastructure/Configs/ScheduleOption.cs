namespace XinjingdailyBot.Infrastructure.Configs;

/// <summary>
/// 任务计划
/// </summary>
public sealed record ScheduleOption
{
    /// <summary>
    /// 任务计划
    /// </summary>
    public Dictionary<string, string> Cron { get; set; } = [];
}
