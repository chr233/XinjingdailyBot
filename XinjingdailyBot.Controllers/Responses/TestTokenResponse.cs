using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.WebAPI.IPC.Responses;

/// <summary>
/// 连接测试响应
/// </summary>
public sealed record TestTokenResponse
{
    /// <summary>
    /// 用户名@xxx
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 用户表ID
    /// </summary>
    public int UID { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? NickName { get; set; }
    /// <inheritdoc cref="EUserRights"/>
    public EUserRights UserRight { get; set; }
    /// <summary>
    /// 权限组ID
    /// </summary>
    public int GroupId { get; set; }
}
