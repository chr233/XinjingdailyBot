using DsNext.Infrastructure.Options;

namespace XinjingdailyBot.Infrastructure.Options;
/// <summary>
/// 频道设置
/// </summary>
public sealed record ChannelOption : IXjbConfig
{
    static string? IXjbConfig.SectionName => "Channel";

    /// <summary>
    /// 审核群组
    /// </summary>
    public string ReviewGroup { get; set; } = "";





    /// <summary>
    /// 日志频道
    /// </summary>
    public string LogChannel { get; set; } = "";
    /// <summary>
    /// 是否使用审核日志模式
    /// 启用: 审核后在审核群直接删除消息, 审核记录发送至审核日志频道
    /// 禁用: 审核后再审核群保留消息记录, 审核日志频道不使用
    /// </summary>
    public bool UseReviewLogMode { get; set; }
    /// <summary>
    /// 频道评论区群组
    /// </summary>
    public string CommentGroup { get; set; } = "";
    /// <summary>
    /// 闲聊区群组
    /// </summary>
    public string SubGroup { get; set; } = "";
    /// <summary>
    /// 通过频道
    /// </summary>
    public string AcceptChannel { get; set; } = "";
    /// <summary>
    /// 第二频道
    /// </summary>
    public string SecondChannel { get; set; } = "";
    /// <summary>
    /// 第二频道评论区
    /// </summary>
    public string SecondCommentGroup { get; set; } = "";
    /// <summary>
    /// 拒稿频道
    /// </summary>
    public string RejectChannel { get; set; } = "";
    /// <summary>
    /// 管理日志频道
    /// 用于存储封禁/解封日志
    /// </summary>
    public string AdminLogChannel { get; set; } = "";


}
