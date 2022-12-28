using System.Text;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Command.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class NormalCommand
    {
        private readonly ILogger<NormalCommand> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly LevelRepository _levelRepository;
        private readonly GroupRepository _groupRepository;

        public NormalCommand(
            ILogger<NormalCommand> logger,
            ITelegramBotClient botClient,
            IUserService userService,
            LevelRepository levelRepository,
            GroupRepository groupRepository)
        {
            _logger = logger;
            _botClient = botClient;
            _userService = userService;
            _levelRepository = levelRepository;
            _groupRepository = groupRepository;
        }

        /// <summary>
        /// 检测机器人是否存活
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("PING", UserRights.NormalCmd, Description = "检测机器人是否存活")]
        public async Task ResponsePing(Message message)
        {
            await _botClient.SendCommandReply("PONG!", message);
        }

        /// <summary>
        /// 设置是否匿名
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("ANONYMOUS", UserRights.NormalCmd, Alias = "ANYMOUSE", Description = "设置是否匿名")]
        public async Task ResponseAnonymous(Users dbUser, Message message)
        {
            bool anymouse = !dbUser.PreferAnonymous;
            dbUser.PreferAnonymous = anymouse;
            dbUser.ModifyAt = DateTime.Now;
            await _userService.Updateable(dbUser).UpdateColumns(x => new { x.PreferAnonymous, x.ModifyAt }).ExecuteCommandAsync();

            string mode = anymouse ? "匿名投稿" : "保留来源";
            string text = $"后续投稿将默认使用【{mode}】";
            await _botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 设置稿件审核后是否通知
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("NOTIFICATION", UserRights.NormalCmd, Description = "设置稿件审核后是否通知")]
        public async Task ResponseNotification(Users dbUser, Message message)
        {
            bool notificationg = !dbUser.Notification;
            dbUser.Notification = notificationg;
            dbUser.ModifyAt = DateTime.Now;
            await _userService.Updateable(dbUser).UpdateColumns(x => new { x.Notification, x.ModifyAt }).ExecuteCommandAsync();

            string mode = notificationg ? "接收通知" : "静默模式";
            string text = $"稿件被审核或者过期时将会尝试通知用户\n当前通知设置: {mode}";
            await _botClient.SendCommandReply(text, message);
        }

        /// <summary>
        /// 获取自己的信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MYINFO", UserRights.NormalCmd, Description = "获取自己的信息")]
        public async Task ResponseMyInfo(Users dbUser, Message message)
        {
            string userNick = message.From!.EscapedNickName();
            string level = _levelRepository.GetLevelName(dbUser.Level);
            string group = _groupRepository.GetGroupName(dbUser.GroupID);

            int totalPost = dbUser.PostCount - dbUser.ExpiredPostCount;

            StringBuilder sb = new();

            sb.AppendLine("-- 基础信息 --");
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户ID: <code>{dbUser.UserID}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"等级:  <code>{level}</code>");
            sb.AppendLine($"投稿数量: <code>{totalPost}</code>");
            sb.AppendLine($"投稿通过率: <code>{(100.0 * dbUser.AcceptCount / totalPost).ToString("0.00")}%</code>");
            sb.AppendLine($"通过数量: <code>{dbUser.AcceptCount}</code>");
            sb.AppendLine($"拒绝数量: <code>{dbUser.RejetCount}</code>");
            sb.AppendLine($"审核数量: <code>{dbUser.ReviewCount}</code>");
            sb.AppendLine();
            sb.AppendLine("-- 用户排名 --");

            DateTime now = DateTime.Now;
            DateTime prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            if (dbUser.GroupID == 1)
            {
                if (dbUser.AcceptCount >= 1)
                {
                    const int miniumPost = 10;

                    int acceptCountRank = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > dbUser.AcceptCount && x.ModifyAt >= prev30Days).CountAsync() + 1;

                    double ratio = 1.0 * dbUser.AcceptCount / dbUser.PostCount;
                    int acceptRatioRank = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                    .Select(y => 100.0 * y.AcceptCount / y.PostCount).Where(x => x > ratio).CountAsync() + 1;

                    sb.AppendLine($"通过数排名: <code>{acceptCountRank}</code>");
                    sb.AppendLine($"通过率排名: <code>{acceptRatioRank}</code>");
                }
                else
                {
                    sb.AppendLine("稿件数量太少, 未进入排行榜");
                }
            }
            else
            {
                int activeUser = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.ModifyAt >= prev30Days).CountAsync();
                sb.AppendLine($"活跃用户数: <code>{activeUser}</code>");

                sb.AppendLine($"管理员不参与用户排名");
                sb.AppendLine($"可以使用命令 /userrank 查看总排名");
            }

            await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }


        /// <summary>
        /// 获取自己的权限
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MYRIGHT", UserRights.NormalCmd, Description = "获取自己的权限")]
        public async Task ResponseMyRight(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            var right = dbUser.Right;
            bool superCmd = right.HasFlag(UserRights.SuperCmd);
            bool adminCmd = right.HasFlag(UserRights.AdminCmd);
            bool normalCmd = right.HasFlag(UserRights.NormalCmd);
            bool sendPost = right.HasFlag(UserRights.SendPost);
            bool reviewPost = right.HasFlag(UserRights.ReviewPost);
            bool directPost = right.HasFlag(UserRights.DirectPost);
            string userNick = message.From!.EscapedNickName();

            string group = _groupRepository.GetGroupName(dbUser.GroupID);

            List<string> functions = new();
            if (sendPost) { functions.Add("投递稿件"); }
            if (reviewPost) { functions.Add("审核稿件"); }
            if (directPost) { functions.Add("直接发布稿件"); }
            if (functions.Count == 0) { functions.Add("无"); }

            List<string> commands = new();
            if (superCmd) { commands.Add("所有命令"); }
            if (adminCmd) { commands.Add("管理员命令"); }
            if (normalCmd) { commands.Add("普通命令"); }
            if (functions.Count == 0) { commands.Add("无"); }

            StringBuilder sb = new();
            sb.AppendLine($"用户名: <code>{userNick}</code>");
            sb.AppendLine($"用户组: <code>{group}</code>");
            sb.AppendLine($"功能: <code>{string.Join(", ", functions)}</code>");
            sb.AppendLine($"命令: <code>{string.Join(", ", commands)}</code>");

            await botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 艾特群管理
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("ADMIN", UserRights.NormalCmd, Description = "艾特群管理")]
        public async Task ResponseCallAdmins(Message message)
        {
            StringBuilder sb = new();

            if (message.Chat.Type != ChatType.Group && message.Chat.Type != ChatType.Supergroup)
            {
                sb.AppendLine("该命令仅在群组内有效");
            }
            else
            {
                ChatMember[] admins = await _botClient.GetChatAdministratorsAsync(message.Chat.Id);

                foreach (var menber in admins)
                {
                    var admin = menber.User;
                    if (!(admin.IsBot || string.IsNullOrEmpty(admin.Username)))
                    {
                        sb.AppendLine($"@{admin.Username}");
                    }
                }
            }

            await _botClient.SendCommandReply(sb.ToString(), message);
        }

    }
}
