using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Handlers.Messages.Commands
{
    internal static class CommonCmd
    {
        //命令列表
        private static Dictionary<string, string> NormalCmds { get; } = new()
        {
            { "anonymous", "设置投稿是否默认匿名" },
            { "notification", "设置是否开启投稿通知" },
            { "admin", "呼叫群管理" },
            { "myinfo", "查询投稿数量" },
            { "myright", "查询权限信息" },
            { "ping", "测试机器人是否存活" },
        };

        private static Dictionary<string, string> AdminCmds { get; } = new()
        {
            { "userinfo", "查询用户信息" },
            { "ban", "封禁指定用户" },
            { "unban", "解封指定用户" },
            { "queryban", "查询封禁记录" },
        };

        private static Dictionary<string, string> SuperCmds { get; } = new()
        {
            { "restart", "重启机器人" },
        };

        private static Dictionary<string, string> ReviewCmds { get; } = new()
        {
            { "no", "自定义拒稿理由" },
            { "edit", "修改稿件描述" },
        };

        private static Dictionary<string, string> CommonCmds { get; } = new()
        {
            { "myban", "查询封禁记录" },
        };

        /// <summary>
        /// 显示命令帮助
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseHelp(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;
            bool review = dbUser.Right.HasFlag(UserRights.ReviewPost);

            StringBuilder sb = new();

            if (!dbUser.IsBan)
            {
                sb.AppendLine("发送图片/视频或者文字内容即可投稿");

                if (normal)
                {
                    sb.AppendLine();
                    foreach (var cmd in NormalCmds)
                    {
                        sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                    }
                }

                if (admin)
                {
                    sb.AppendLine();
                    foreach (var cmd in AdminCmds)
                    {
                        sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                    }
                }

                if (super)
                {
                    sb.AppendLine();
                    foreach (var cmd in SuperCmds)
                    {
                        sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                    }
                }

                if (review)
                {
                    sb.AppendLine();
                    foreach (var cmd in ReviewCmds)
                    {
                        sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                    }
                }
            }
            else
            {
                sb.AppendLine("您已被限制访问此Bot, 仅可使用以下命令: \n");

                foreach (var cmd in CommonCmds)
                {
                    sb.AppendLine($"/{cmd.Key}  {cmd.Value}");
                }
            }

            await botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 首次欢迎语
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseStart(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            StringBuilder sb = new();

            if (!dbUser.IsBan)
            {
                sb.AppendLine("欢迎使用 心惊报 @xinjingdaily 专用投稿机器人");
                sb.AppendLine("直接发送图片或者文字内容即可投稿");
            }
            else
            {
                sb.AppendLine("您已被限制访问此Bot, 无法使用投稿等功能");
            }

            sb.AppendLine("查看命令帮助: /help");

            await botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 查看机器人版本
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseVersion(ITelegramBotClient botClient, Message message)
        {
            string text = $"机器人版本: {MyVersion}";
            await botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 查询自己是否被封禁
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static async Task ResponseMyBan(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            var records = await DB.Queryable<BanRecords>().Where(x => x.UserID == dbUser.UserID).ToListAsync();

            StringBuilder sb = new();

            string status = dbUser.IsBan ? "已封禁" : "正常";
            sb.AppendLine($"用户名: <code>{dbUser.UserNick}</code>");
            sb.AppendLine($"用户ID: <code>{dbUser.UserID}</code>");
            sb.AppendLine($"状态: <code>{status}</code>");
            sb.AppendLine();

            if (records == null)
            {
                sb.AppendLine("查询封禁/解封记录出错");
            }
            else if (!records.Any())
            {
                sb.AppendLine("尚未查到封禁/解封记录");
            }
            else
            {
                foreach (var record in records)
                {
                    string date = record.BanTime.ToString("d");
                    string operate = record.IsBan ? "受到封禁" : "被解封";
                    sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> {operate}");
                }
            }

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }
    }
}
