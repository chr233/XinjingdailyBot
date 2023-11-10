namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// 
/// </summary>
public static class DatetimeExtensions
{
    /// <summary>
    /// 时间戳起始时间
    /// </summary>
    public static readonly DateTime StartTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local);

    /// <summary>
    /// 转时间戳
    /// </summary>
    /// <param name="timestamp"></param>
    public static long GetTimestamp(this DateTime timestamp)
    {
        long tick = (long)(timestamp - StartTime).TotalSeconds;
        return tick;
    }

    /// <summary>
    /// 转毫秒时间戳
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static long GetTimestampMs(this DateTime timestamp)
    {
        long tick = (long)(timestamp - StartTime).TotalMilliseconds;
        return tick;
    }

    /// <summary>
    /// 时间戳转时间
    /// </summary>
    /// <param name="tick"></param>
    /// <returns></returns>
    public static DateTime GetDateTime(this long tick)
    {
        if (tick < DateTimeOffset.MinValue.Ticks || tick > DateTimeOffset.MaxValue.Ticks)
        {
            tick = 0;
        }

        var offset = DateTimeOffset.FromUnixTimeSeconds(tick);
        return offset.LocalDateTime;
    }

    /// <summary>
    /// 毫秒时间戳转时间
    /// </summary>
    /// <param name="tick"></param>
    /// <returns></returns>
    public static DateTime GetDateTimeMs(this long tick)
    {
        if (tick < DateTimeOffset.MinValue.Ticks || tick > DateTimeOffset.MaxValue.Ticks)
        {
            tick = 0;
        }

        var offset = DateTimeOffset.FromUnixTimeMilliseconds(tick);
        return offset.LocalDateTime;
    }
}
