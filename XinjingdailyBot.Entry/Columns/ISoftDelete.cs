namespace XinjingdailyBot.Entry.Columns;

/// <summary>
/// 软删除
/// </summary>
public interface ISoftDelete
{
    /// <inheritdoc cref="ISoftDelete"/>
    bool IsDeleted { get; set; }
}
