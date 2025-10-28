namespace XinjingdailyBot.Infrastructure.Configs;
/// <summary>
/// 机器人设置
/// </summary>
public sealed record BotConfig
{
    /// <summary>
    /// Telegram Api地址
    /// </summary>
    public string? BaseUrl { get; init; }
    /// <summary>
    /// 机器人Token
    /// </summary>
    public string? BotToken { get; init; }
    /// <summary>
    /// 代理链接, 默认 null
    /// </summary>
    public string? Proxy { get; init; }
    /// <summary>
    /// 忽略机器人离线时的Update
    /// </summary>
    public bool ThrowPendingUpdates { get; init; }
    /// <summary>
    /// 自动退出未在配置文件中定义的群组和频道, 默认 false
    /// </summary>
    public bool AutoLeaveOtherGroup { get; init; }
    /// <summary>
    /// 超级管理员(覆盖数据库配置)
    /// </summary>
    public HashSet<long>? SuperAdmins { get; init; }

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
}
