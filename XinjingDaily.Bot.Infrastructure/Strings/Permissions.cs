using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Infrastructure.Strings;

public static class Permissions
{
    // 命令
    public const string CommonCommand = "CMD:COMMON";
    public const string QueryCommand = "CMD:QUERY";
    public const string NormalCommand = "CMD:QUERY";

    // 投稿
    public const string PostCreate = "post:create";
    public const string PostDelete = "post:delete:own";

    public const string PostReview = "post:review";
    public const string PostViewAll = "post:view:all";

    // 审核
    public const string ReviewPass = "review:pass";
    public const string ReviewReject = "review:reject";
    public const string ReviewComment = "review:comment";

    public const string PostDeleteAny = "post:delete:any";


    // 群管理
    public const string GroupKick = "group:kick";
    public const string GroupBan = "group:ban";
    public const string GroupMute = "group:mute";
    public const string GroupSetting = "group:setting";

    // 娱乐
    public const string GamePlay = "game:play";

    // 机器人
    public const string BotSetting = "bot:setting";
    public const string BotUserRole = "bot:user:role";


    extension(EPermission permission)
    {
        public string ToPermissionString()
        {
            //return permission switch {
            //    EPermission.BotAdmin => BotAdminCommand,
            //    EPermission.Reviewer => ReviewerCommand,
            //    EPermission.Reviewer => GroupAdminCommand,
            //    _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
            //};
            return "";
        }
    }

}

