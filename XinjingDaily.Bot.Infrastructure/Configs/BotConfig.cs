namespace XinjingDaily.Bot.Infrastructure.Configs;

/// <summary>
/// 机器人选项
/// </summary>
public sealed record BotConfig
{
    public string? BotToken { get; set; }

    public string? BotProxy { get; set; }

    /// <summary>
    /// 忽略机器人离线时的Update
    /// </summary>
    public bool ThrowPendingUpdates { get; init; }

    /// <summary>
    /// 自动退出未在配置文件中定义的群组和频道, 默认 false
    /// </summary>
    public bool AutoLeaveOtherGroup { get; init; }

    /// <summary>
    /// 启用定时发布
    /// </summary>
    public bool EnablePlanPost { get; init; }

    /// <summary>
    /// 二级菜单
    /// </summary>
    public bool PostSecondMenu { get; init; }

    /// <summary>
    /// 文本稿件发布时是否启用链接预览
    /// </summary>
    public bool EnableWebPagePreview { get; init; }

    /// <summary>
    /// 纯链接投稿显示警告
    /// </summary>
    public bool WarnRawLinkPost { get; init; }
}
