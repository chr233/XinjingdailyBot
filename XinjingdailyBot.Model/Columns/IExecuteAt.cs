namespace XinjingdailyBot.Model.Columns;

/// <summary>
/// 命令调用时间
/// </summary>
public interface IExecuteAt
{
    /// <inheritdoc cref="IExecuteAt"/>
    DateTime ExecuteAt { get; set; }
}
