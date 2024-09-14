using SqlSugar;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Model.Base;
using XinjingdailyBot.Model.Columns;

namespace XinjingdailyBot.Model.Models;

/// <summary>
/// 用户表, 储存所有用户的基本信息, 权限设定, 以及投稿信息统计
/// </summary>
[SugarTable("user", TableDescription = "用户表")]
[SugarIndex("index_userid", nameof(UserID), OrderByType.Asc, true)]
[SugarIndex("index_username", nameof(UserName), OrderByType.Asc)]
public sealed record Users : BaseModel, IModifyAt, ICreateAt
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserID { get; set; }
    /// <summary>
    /// 用户名@
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? UserName { get; set; }
    /// <summary>
    /// 用户昵称 姓
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string ?FirstName { get; set; }
    /// <summary>
    /// 用户昵称 名
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string ?LastName { get; set; }
    /// <summary>
    /// 用户昵称
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}";
    /// <summary>
    /// 是否封禁
    /// </summary>
    public bool IsBan { get; set; }
    /// <summary>
    /// 是否为Bot
    /// </summary>
    public bool IsBot { get; set; }
    /// <summary>
    /// 是否为高级用户
    /// </summary>
    public bool IsVip { get; set; }

    /// <summary>
    /// 默认开启匿名模式
    /// </summary>
    public bool PreferAnonymous { get; set; }
    /// <summary>
    /// 是否开启通知
    /// </summary>
    public bool Notification { get; set; } = true;
    /// <summary>
    /// 通过的稿件数量
    /// </summary>
    public int AcceptCount { get; set; }
    /// <summary>
    /// 被拒绝的稿件数量
    /// </summary>
    public int RejectCount { get; set; }
    /// <summary>
    /// 过期未被审核的稿件数量(统计时总投稿需要减去此字段)
    /// </summary>
    public int ExpiredPostCount { get; set; }

    /// <summary>
    /// 投稿数量
    /// </summary>
    public int PostCount { get; set; }
    /// <summary>
    /// 审核数量
    /// </summary>
    public int ReviewCount { get; set; }
    /// <summary>
    /// 经验
    /// </summary>
    public ulong Experience { get; set; }
    /// <summary>
    /// 用户等级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 用户组ID
    /// </summary>
    public int GroupID { get; set; }

    /// <inheritdoc cref="ICreateAt"/>
    public DateTime CreateAt { get; set; } = DateTime.Now;
    /// <inheritdoc cref="IModifyAt"/>
    public DateTime ModifyAt { get; set; } = DateTime.Now;

    /// <summary>
    /// API Token
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(Id))]//一对一 SchoolId是StudentA类里面的
    public UserTokens? Token { get; set; } //不能赋值只能是null

    /// <summary>
    /// 文本显示
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(UserName))
        {
            return $"{FullName}(#{UserID})".EscapeHtml();
        }
        else
        {
            return $"{FullName}(@{UserName})".EscapeHtml();
        }
    }

    /// <summary>
    /// Html链接
    /// </summary>
    /// <returns></returns>
    public string HtmlUserLink()
    {
        string nick = FullName.EscapeHtml();

        if (string.IsNullOrEmpty(UserName))
        {
            return $"<a href=\"tg://user?id={UserID}\">{nick}</a>";
        }
        else
        {
            return $"<a href=\"https://t.me/{UserName}\">{nick}</a>";
        }
    }

    /// <summary>
    /// 用户名转义
    /// </summary>
    /// <returns></returns>
    public string EscapedFullName()
    {
        return FullName.EscapeHtml();
    }
}
