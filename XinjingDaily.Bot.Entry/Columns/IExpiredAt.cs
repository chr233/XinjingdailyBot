namespace XinjingDaily.Bot.Entry.Columns;

/// <summary>
/// 过期时间
/// </summary>
public interface IExpiredAt
{
    /// <inheritdoc cref="IExpiredAt"/>
    DateTime ExpiredAt { get; set; }
}
