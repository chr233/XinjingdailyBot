namespace XinjingdailyBot.Model.Columns;

/// <summary>
/// 软删除
/// </summary>
public interface ISoftDelete
{
    /// <inheritdoc cref="ISoftDelete"/>
    bool IsDeleted { get; set; }
}
