namespace XinjingdailyBot.Model.Columns;

/// <summary>
/// 创建时间修改时间
/// </summary>
public interface ICreateModifyAt
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreateAt { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    DateTime ModifyAt { get; set; }
}
