using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Infrastructure.Strings;

public static class Permissions
{
    public const string BotAdminCommand = "bot-admin:command";
    public const string ReviewerCommand = "reviewer:command";
    public const string GroupAdminCommand = "group-admin:command";


    extension(EPermission permission)
    {
        public string ToPermissionString()
        {
            return permission switch
            {
                EPermission.BotAdmin => BotAdminCommand,
                EPermission.Reviewer => ReviewerCommand,
                EPermission.GroupAdmin => GroupAdminCommand,
                _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null)
            };
        }
    }

}

