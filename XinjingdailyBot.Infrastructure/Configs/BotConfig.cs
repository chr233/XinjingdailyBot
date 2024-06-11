using DsNext.Infrastructure.Options;

namespace XinjingdailyBot.Infrastructure.Options;
public sealed record BotConfig : IXjbConfig
{
    static string? IXjbConfig.SectionName => "Bot";

    /// <summary>
    /// Telegram Api地址
    /// </summary>
    public string? BaseUrl { get; set; }
    /// <summary>
    /// 机器人Token
    /// </summary>
    public List<string?> BotTokens { get; set; }
    /// <summary>
    /// 机器人Token
    /// </summary>
    public string BotToken { get; set; }
    /// <summary>
    /// 代理链接, 默认 null
    /// </summary>
    public string? Proxy { get; set; }
    /// <summary>
    /// 忽略机器人离线时的Update
    /// </summary>
    public bool ThrowPendingUpdates { get; set; }
    /// <summary>
    /// 自动退出未在配置文件中定义的群组和频道, 默认 false
    /// </summary>
    public bool AutoLeaveOtherGroup { get; set; }
    /// <summary>
    /// 超级管理员(覆盖数据库配置)
    /// </summary>
    public HashSet<long>? SuperAdmins { get; set; }

    /// <summary>
    /// 启用定时发布
    /// </summary>
    public bool EnablePlanPost { get; set; }
    /// <summary>
    /// 二级菜单
    /// </summary>
    public bool PostSecondMenu { get; set; }
    /// <summary>
    /// 文本稿件发布时是否启用链接预览
    /// </summary>
    public bool EnableWebPagePreview { get; set; }
    /// <summary>
    /// 纯链接投稿显示警告
    /// </summary>
    public bool WarnRawLinkPost { get; set; }
}
