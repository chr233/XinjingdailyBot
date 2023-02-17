using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Command
{
    [AppService(LifeTime.Scoped)]
    public class AdminCommand
    {
        private readonly ILogger<AdminCommand> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        //private readonly LevelRepository _levelRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IBanRecordService _banRecordService;
        private readonly IPostService _postService;
        private readonly IAttachmentService _attachmentService;
        private readonly IChannelOptionService _channelOptionService;
        private readonly IChannelService _channelService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly ICmdRecordService _cmdRecordService;

        public AdminCommand(
            ILogger<AdminCommand> logger,
            ITelegramBotClient botClient,
            IUserService userService,
            //LevelRepository levelRepository,
            GroupRepository groupRepository,
            IBanRecordService banRecordService,
            IPostService postService,
            IAttachmentService attachmentService,
            IChannelOptionService channelOptionService,
            IChannelService channelService,
            IMarkupHelperService markupHelperService,
            ICmdRecordService cmdRecordService)
        {
            _logger = logger;
            _botClient = botClient;
            _userService = userService;
            //_levelRepository = levelRepository;
            _groupRepository = groupRepository;
            _banRecordService = banRecordService;
            _postService = postService;
            _attachmentService = attachmentService;
            _channelOptionService = channelOptionService;
            _channelService = channelService;
            _markupHelperService = markupHelperService;
            _cmdRecordService = cmdRecordService;
        }

        private readonly DateTime StartAt = DateTime.Now;

        /// <summary>
        /// 被警告超过此值自动封禁
        /// </summary>
        private const int WarningLimit = 3;

        /// <summary>
        /// 获取群组信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("GROUPINFO", UserRights.AdminCmd, Description = "获取群组信息")]
        public async Task ResponseGroupInfo(Message message)
        {
            var chat = message.Chat;

            StringBuilder sb = new();

            if (chat.Type != ChatType.Group && chat.Type != ChatType.Supergroup)
            {
                sb.AppendLine("该命令仅限群组内使用");
            }
            else
            {
                var groupTitle = chat.EscapedChatName();
                sb.AppendLine($"群组名: <code>{groupTitle ?? "无"}</code>");

                if (string.IsNullOrEmpty(chat.Username))
                {
                    sb.AppendLine("群组类型: <code>私聊</code>");
                    sb.AppendLine($"群组ID: <code>{chat.Id}</code>");
                }
                else
                {
                    sb.AppendLine("群组类型: <code>公开</code>");
                    sb.AppendLine($"群组链接: <code>@{chat.Username ?? "无"}</code>");
                }
            }
            await _botClient.SendCommandReply(sb.ToString(), message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("USERINFO", UserRights.AdminCmd, Alias = "UINFO", Description = "获取用户信息")]
        public async Task ResponseUserInfo(Message message, string[] args)
        {
            StringBuilder sb = new();

            var targetUser = await _userService.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args.Any())
                {
                    targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                }
            }

            if (targetUser == null)
            {
                sb.AppendLine("找不到指定用户, 你可以手动指定用户名/用户ID");
            }
            else
            {
                sb.AppendLine("-- 基础信息 --");
                sb.AppendLine(_userService.GetUserBasicInfo(targetUser));

                sb.AppendLine();
                sb.AppendLine("-- 用户排名 --");
                sb.AppendLine(await _userService.GetUserRank(targetUser));
            }

            await _botClient.SendCommandReply(sb.ToString(), message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 封禁用户
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("BAN", UserRights.AdminCmd, Description = "封禁用户")]
        public async Task ResponseBan(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await _userService.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                var reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定封禁理由";
                }

                if (targetUser.IsBan)
                {
                    return "当前用户已经封禁, 请不要重复操作";
                }
                else
                {
                    targetUser.IsBan = true;
                    targetUser.ModifyAt = DateTime.Now;
                    await _userService.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                    var record = new BanRecords
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.Ban,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await _banRecordService.Insertable(record).ExecuteCommandAsync();

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            var msg = string.Format(Langs.BanedUserTips, "管理员", reason);
                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            _logger.LogInformation("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("发送私聊消息失败 {Message}", ex.Message);
                    }

                    StringBuilder sb = new();
                    sb.AppendLine($"成功封禁 {targetUser.HtmlUserLink()}");
                    sb.AppendLine($"操作员 {dbUser.HtmlUserLink()}");
                    sb.AppendLine($"封禁理由 <code>{reason}</code>");
                    return sb.ToString();
                }
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 解封用户
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("UNBAN", UserRights.AdminCmd, Description = "解封用户")]
        public async Task ResponseUnban(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await _userService.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                var reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定解封理由";
                }

                if (!targetUser.IsBan)
                {
                    return "当前用户未被封禁或者已经解封, 请不要重复操作";
                }
                else
                {
                    targetUser.IsBan = false;
                    targetUser.ModifyAt = DateTime.Now;
                    await _userService.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                    var record = new BanRecords
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.UnBan,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await _banRecordService.Insertable(record).ExecuteCommandAsync();

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            var msg = string.Format(Langs.UnbanedUserTips, "管理员", reason);
                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            _logger.LogInformation("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("发送私聊消息失败 {Message}", ex.Message);
                    }

                    StringBuilder sb = new();
                    sb.AppendLine($"成功解封 {targetUser.HtmlUserLink()}");
                    sb.AppendLine($"操作员 {dbUser.HtmlUserLink()}");
                    sb.AppendLine($"解封理由 <code>{reason}</code>");
                    return sb.ToString();
                }
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 警告用户
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("WARN", UserRights.AdminCmd, Description = "警告用户")]
        public async Task ResponseWarning(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                var targetUser = await _userService.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                var reason = string.Join(' ', args);
                if (string.IsNullOrEmpty(reason))
                {
                    return "请指定警告理由";
                }

                if (targetUser.IsBan)
                {
                    return "当前用户已被封禁, 无法再发送警告";
                }
                else
                {
                    //获取最近一条解封记录
                    var lastUnbaned = await _banRecordService.Queryable().Where(x => x.UserID == targetUser.UserID && (x.Type == BanType.UnBan || x.Type == BanType.Ban))
                        .OrderByDescending(x => x.BanTime).FirstAsync();

                    int warnCount;
                    if (lastUnbaned == null)
                    {
                        warnCount = await _banRecordService.Queryable().Where(x => x.UserID == targetUser.UserID && x.Type == BanType.Warning).CountAsync();
                    }
                    else
                    {
                        warnCount = await _banRecordService.Queryable().Where(x => x.UserID == targetUser.UserID && x.Type == BanType.Warning && x.BanTime >= lastUnbaned.BanTime).CountAsync();
                    }

                    var record = new BanRecords
                    {
                        UserID = targetUser.UserID,
                        OperatorUID = dbUser.UserID,
                        Type = BanType.Warning,
                        BanTime = DateTime.Now,
                        Reason = reason,
                    };

                    await _banRecordService.Insertable(record).ExecuteCommandAsync();

                    warnCount++;

                    StringBuilder sb = new();
                    sb.AppendLine($"成功警告 {targetUser.HtmlUserLink()}");
                    sb.AppendLine($"操作员 {dbUser.HtmlUserLink()}");
                    sb.AppendLine($"警告理由 <code>{reason}</code>");
                    sb.AppendLine($"累计警告 <code>{warnCount}</code> / <code>{WarningLimit}</code> 次");

                    if (warnCount >= WarningLimit)
                    {
                        record = new BanRecords
                        {
                            UserID = targetUser.UserID,
                            OperatorUID = 0,
                            Type = BanType.Ban,
                            BanTime = DateTime.Now,
                            Reason = "受到警告过多, 自动封禁",
                        };

                        await _banRecordService.Insertable(record).ExecuteCommandAsync();

                        targetUser.IsBan = true;
                        targetUser.ModifyAt = DateTime.Now;
                        await _userService.Updateable(targetUser).UpdateColumns(x => new { x.IsBan, x.ModifyAt }).ExecuteCommandAsync();

                        sb.AppendLine($"受到警告过多, 系统自动封禁该用户");
                    }

                    try
                    {
                        if (targetUser.PrivateChatID > 0)
                        {
                            StringBuilder s = new();
                            s.AppendLine(string.Format(Langs.WarnUserTips, reason));
                            s.AppendLine(string.Format(Langs.WarnUserTips2, warnCount, WarningLimit));
                            string msg = s.ToString();
                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg, parseMode: ParseMode.Html);

                            if (warnCount >= WarningLimit)
                            {
                                msg = string.Format(Langs.BanedUserTips, "系统", "自动封禁");
                                await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, msg);
                            }

                            await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, Langs.QueryBanDescribe);
                        }
                        else
                        {
                            _logger.LogInformation("用户未私聊过机器人, 无法发送消息");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("发送私聊消息失败 {Message}", ex.Message);
                    }

                    return sb.ToString();
                }
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 查询封禁记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("QUERYBAN", UserRights.AdminCmd, Alias = "QBAN", Description = "查询封禁记录")]
        public async Task ResponseQueryBan(Message message, string[] args)
        {
            StringBuilder sb = new();

            var targetUser = await _userService.FetchTargetUser(message);

            if (targetUser == null)
            {
                if (args.Any())
                {
                    targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                }
            }

            if (targetUser == null)
            {
                sb.AppendLine("找不到指定用户");
            }
            else
            {
                var records = await _banRecordService.Queryable().Where(x => x.UserID == targetUser.UserID).ToListAsync();

                var status = targetUser.IsBan ? "已封禁" : "正常";
                sb.AppendLine($"用户名: <code>{targetUser.EscapedFullName()}</code>");
                sb.AppendLine($"用户ID: <code>{targetUser.UserID}</code>");
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
                    var operators = records.Select(x => x.OperatorUID).Distinct();

                    var users = await _userService.Queryable().Where(x => operators.Contains(x.UserID)).Distinct().ToListAsync();

                    foreach (var record in records)
                    {
                        var date = record.BanTime.ToString("d");
                        var operate = record.Type switch
                        {
                            BanType.UnBan => "解封",
                            BanType.Ban => "封禁",
                            BanType.Warning => "警告",
                            _ => "未知",
                        };

                        string admin;
                        if (record.OperatorUID == 0)
                        {
                            admin = "系统";
                        }
                        else
                        {
                            var user = users.Find(x => x.UserID == record.OperatorUID);
                            admin = user != null ? user.EscapedFullName() : record.OperatorUID.ToString();
                        }

                        sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> 被 <code>{admin}</code> {operate}");
                    }
                }
            }

            await _botClient.SendCommandReply(sb.ToString(), message, false, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 回复用户
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("ECHO", UserRights.AdminCmd, Description = "回复用户")]
        public async Task ResponseEcho(Users dbUser, Message message, string[] args)
        {
            var autoDelete = true;
            async Task<string> exec()
            {
                var targetUser = await _userService.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                        args = args[1..];
                    }
                }

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.UserID == dbUser.UserID)
                {
                    return "为什么有人想要自己回复自己?";
                }

                if (targetUser.PrivateChatID <= 0)
                {
                    return "该用户尚未私聊过机器人, 无法发送消息";
                }

                var msg = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(msg))
                {
                    return "请输入回复内容";
                }

                autoDelete = false;
                try
                {
                    await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, $"来自管理员的消息:\n<code>{msg.EscapeHtml()}</code>", parseMode: ParseMode.Html);
                    return "消息发送成功";
                }
                catch (Exception ex)
                {
                    return $"消息发送失败 {ex.Message}";
                }
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, autoDelete);
        }

        /// <summary>
        /// 搜索用户
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("SEARCHUSER", UserRights.AdminCmd, Alias = "QUSER", Description = "搜索用户")]
        public async Task ResponseSearchUser(Users dbUser, Message message, string[] args)
        {
            if (!args.Any())
            {
                await _botClient.SendCommandReply("请指定 用户昵称/用户名/用户ID 作为查询参数", message, true);
                return;
            }

            var query = args.First();

            int page;
            if (args.Length >= 2)
            {
                if (!int.TryParse(args[1], out page))
                {
                    page = 1;
                }
            }
            else
            {
                page = 1;
            }

            (var text, var keyboard) = await _userService.QueryUserList(dbUser, query, page);

            await _botClient.SendCommandReply(text, message, false, ParseMode.Html, keyboard);
        }

        /// <summary>
        /// 搜索用户翻页
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("SEARCHUSER", UserRights.AdminCmd, Alias = "QUERYUSER")]
        public async Task QResponseSearchUser(Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                if (args.Length < 3)
                {
                    return ("参数有误", null);
                }

                string query = args[1];

                if (!int.TryParse(args[2], out int page))
                {
                    page = 1;
                }

                return await _userService.QueryUserList(dbUser, query, page);
            }
            (string text, var kbd) = await exec();
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, text, ParseMode.Html, true, kbd);
        }

        /// <summary>
        /// 停止搜索用户
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("CANCELSEARCHUSER", UserRights.AdminCmd, Alias = "CANCELCLOSE")]
        public async Task QResponseCancelSearchUser(CallbackQuery callbackQuery, string[] args)
        {
            string text = args.Length >= 2 ? args[1] : "参数有误";
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, text, ParseMode.Html, true, null);
        }

        /// <summary>
        /// 生成投稿统计信息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("POSTREPORT", UserRights.AdminCmd, Alias = "POSTSTATUS", Description = "生成投稿统计信息")]
        public async Task ResponsePostReport(Message message)
        {
            var now = DateTime.Now;
            var prev1Day = now.AddDays(-1).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var prev7Days = now.AddDays(-7).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var monthStart = now.AddDays(1 - now.Day).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);
            var yearStart = now.AddMonths(1 - now.Month).AddDays(1 - now.Day).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            StringBuilder sb = new();

            var todayPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status > PostStatus.Cancel).CountAsync();
            var todayAcceptPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == PostStatus.Accepted).CountAsync();
            var todayRejectPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status == PostStatus.Rejected).CountAsync();
            var todayExpiredPost = await _postService.Queryable().Where(x => x.CreateAt >= prev1Day && x.Status < 0).CountAsync();

            sb.AppendLine("-- 24小时投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{todayAcceptPost}</code> / <code>{todayRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(todayPost > 0 ? (100 * todayAcceptPost / todayPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{todayExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{todayPost + todayExpiredPost}</code>");

            var weekPost = await _postService.Queryable().Where(x => x.CreateAt >= prev7Days && x.Status > PostStatus.Cancel).CountAsync();
            var weekAcceptPost = await _postService.Queryable().Where(x => x.CreateAt >= prev7Days && x.Status == PostStatus.Accepted).CountAsync();
            var weekRejectPost = await _postService.Queryable().Where(x => x.CreateAt >= prev7Days && x.Status == PostStatus.Rejected).CountAsync();
            var weekExpiredPost = await _postService.Queryable().Where(x => x.CreateAt >= prev7Days && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 7日投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{weekAcceptPost}</code> / <code>{weekRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(weekPost > 0 ? (100 * weekAcceptPost / weekPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{weekExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{weekPost + weekExpiredPost}</code>");

            var monthPost = await _postService.Queryable().Where(x => x.CreateAt >= monthStart && x.Status > PostStatus.Cancel).CountAsync();
            var monthAcceptPost = await _postService.Queryable().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Accepted).CountAsync();
            var monthRejectPost = await _postService.Queryable().Where(x => x.CreateAt >= monthStart && x.Status == PostStatus.Rejected).CountAsync();
            var monthExpiredPost = await _postService.Queryable().Where(x => x.CreateAt >= monthStart && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine($"-- {monthStart.ToString("MM")}月投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{monthAcceptPost}</code> / <code>{monthRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(monthPost > 0 ? (100 * monthAcceptPost / monthPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{monthExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{monthPost}</code>");

            var yearPost = await _postService.Queryable().Where(x => x.CreateAt >= yearStart && x.Status > PostStatus.Cancel).CountAsync();
            var yearAcceptPost = await _postService.Queryable().Where(x => x.CreateAt >= yearStart && x.Status == PostStatus.Accepted).CountAsync();
            var yearRejectPost = await _postService.Queryable().Where(x => x.CreateAt >= yearStart && x.Status == PostStatus.Rejected).CountAsync();
            var yearExpiredPost = await _postService.Queryable().Where(x => x.CreateAt >= yearStart && x.Status < 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine($"-- {yearStart.ToString("yyyy")}年投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{yearAcceptPost}</code> / <code>{yearRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(yearPost > 0 ? (100 * yearAcceptPost / yearPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{yearExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{yearPost}</code>");

            var totalPost = await _postService.Queryable().Where(x => x.Status > PostStatus.Cancel).CountAsync();
            var totalAcceptPost = await _postService.Queryable().Where(x => x.Status == PostStatus.Accepted).CountAsync();
            var totalRejectPost = await _postService.Queryable().Where(x => x.Status == PostStatus.Rejected).CountAsync();
            var totalExpiredPost = await _postService.Queryable().Where(x => x.Status < 0).CountAsync();
            var totalChannel = await _channelOptionService.Queryable().CountAsync();
            var totalAttachment = await _attachmentService.Queryable().CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 历史投稿统计 --");
            sb.AppendLine($"接受/拒绝: <code>{totalAcceptPost}</code> / <code>{totalRejectPost}</code>");
            sb.AppendLine($"通过率: <code>{(totalPost > 0 ? (100 * totalAcceptPost / totalPost).ToString("f2") : "--")}%</code>");
            sb.AppendLine($"过期投稿: <code>{totalExpiredPost}</code>");
            sb.AppendLine($"累计投稿: <code>{totalPost}</code>");
            sb.AppendLine($"频道总数: <code>{totalChannel}</code>");
            sb.AppendLine($"附件总数: <code>{totalAttachment}</code>");

            var totalUser = await _userService.Queryable().CountAsync();
            var banedUser = await _userService.Queryable().Where(x => x.IsBan).CountAsync();
            var weekActiveUser = await _userService.Queryable().Where(x => x.ModifyAt >= prev7Days).CountAsync();
            var MonthActiveUser = await _userService.Queryable().Where(x => x.ModifyAt >= prev30Days).CountAsync();
            var postedUser = await _userService.Queryable().Where(x => x.PostCount > 0).CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 用户统计 --");
            sb.AppendLine($"封禁用户: <code>{banedUser}</code>");
            sb.AppendLine($"周活用户: <code>{weekActiveUser}</code>");
            sb.AppendLine($"月活用户: <code>{MonthActiveUser}</code>");
            sb.AppendLine($"投稿用户: <code>{postedUser}</code>");
            sb.AppendLine($"累计用户: <code>{totalUser}</code>");

            await _botClient.SendCommandReply(sb.ToString(), message, true, ParseMode.Html);
        }

        /// <summary>
        /// 生成系统报表
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("SYSREPORT", UserRights.AdminCmd, Alias = "SYSSTATUS", Description = "生成系统报表")]
        public async Task ResponseSystemReport(Message message)
        {
            StringBuilder sb = new();

            var version = Assembly.Load("XinjingdailyBot.WebAPI").GetName().Version;

            sb.AppendLine("-- 应用信息 --");
            var proc = Process.GetCurrentProcess();
            var mem = proc.WorkingSet64 / 1024.0 / 1024.0;
            sb.AppendLine($"当前版本: <code>{version}</code>");
            sb.AppendLine($"占用内存: <code>{mem:F2}</code> MB");

            TimeSpan uptime = DateTime.Now - StartAt;
            int day = (int)uptime.TotalDays;
            double hours = uptime.TotalHours - day * 24;

            sb.AppendLine($"运行时间: <code>{day}</code> 天 <code>{hours:F8}</code> 小时");

            var today = DateTime.Now.AddHours(-24);
            var cmdCount = await _cmdRecordService.Queryable().Where(x => !x.IsQuery && x.Handled && x.ExecuteAt >= today).CountAsync();
            var QueryCount = await _cmdRecordService.Queryable().Where(x => x.IsQuery && x.Handled && x.ExecuteAt >= today).CountAsync();
            var errorCount = await _cmdRecordService.Queryable().Where(x => x.Error && x.Handled && x.ExecuteAt >= today).CountAsync();

            sb.AppendLine();
            sb.AppendLine("-- 调用统计 --");
            sb.AppendLine($"文字命令: <code>{cmdCount}</code> 次");
            sb.AppendLine($"查询调用: <code>{QueryCount}</code> 次");
            sb.AppendLine($"出错次数: <code>{errorCount}</code> 次");

            sb.AppendLine();
            sb.AppendLine("-- 硬盘信息 --");
            var drives = DriveInfo.GetDrives();

            string[] sizes = new[] { " B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB" };

            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.TotalSize > 0)
                {
                    double usedSize = drive.TotalSize - drive.TotalFreeSpace;
                    double totalSize = drive.TotalSize;
                    int i = 0;
                    while (totalSize > 1024 && i < sizes.Length - 1)
                    {
                        usedSize /= 1024;
                        totalSize /= 1024;
                        i++;
                    }
                    double freeSpacePerc = (drive.TotalSize - drive.TotalFreeSpace) * 100.0 / drive.TotalSize;

                    sb.AppendLine($"{drive.Name} - {drive.DriveFormat} - [ <code>{usedSize:0.00}</code> / <code>{totalSize:0.00}</code> {sizes[i]} <code>{freeSpacePerc:0.0}%</code> ] ");
                }
            }

            sb.AppendLine();
            sb.AppendLine("-- 系统环境 --");
            sb.AppendLine($"框架版本: <code>DotNet {Environment.Version} {RuntimeInformation.OSArchitecture}</code>");
            sb.AppendLine($"系统信息: <code>{RuntimeInformation.OSDescription}</code>");

            await _botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
        }

        /// <summary>
        /// 创建审核群的邀请链接
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("INVITE", UserRights.AdminCmd, Description = "创建审核群的邀请链接")]
        public async Task ResponseInviteToReviewGroup(Users dbUser, Message message)
        {
            if (_channelService.ReviewGroup.Id == -1)
            {
                await _botClient.SendCommandReply("尚未设置审核群, 无法完成此操作", message);
                return;
            }

            if (message.Chat.Type != ChatType.Private)
            {
                await _botClient.SendCommandReply("该命令仅限私聊使用", message);
                return;
            }

            try
            {
                var inviteLink = await _botClient.CreateChatInviteLinkAsync(_channelService.ReviewGroup.Id, $"{dbUser} 创建的邀请链接", DateTime.Now.AddHours(1), 1, false);

                StringBuilder sb = new();

                sb.AppendLine($"创建 {_channelService.ReviewGroup.Title} 的邀请链接成功, 一小时内有效, 仅限1人使用");

                sb.AppendLine($"<a href=\"{inviteLink.InviteLink}\">{(inviteLink.Name ?? inviteLink.InviteLink).EscapeHtml()}</a>");

                await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
            }
            catch
            {
                await _botClient.SendCommandReply("创建邀请链接失败, 可能未给予机器人邀请权限", message);
                throw;
            }
        }

        /// <summary>
        /// 查看用户排行榜
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("USERRANK", UserRights.AdminCmd, Alias = "URANK", Description = "查看用户排行榜")]
        public async Task ResponseUserRank(Message message)
        {
            var now = DateTime.Now;
            var prev30Days = now.AddDays(-30).AddHours(-now.Hour).AddMinutes(-now.Minute).AddSeconds(-now.Second);

            const int topCount = 8;
            const int miniumPost = 10;

            StringBuilder sb = new();

            sb.AppendLine("-- 用户投稿数量排名 --");
            var userAcceptCountRank = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID == 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.AcceptCount).Take(topCount).ToListAsync();
            if (userAcceptCountRank?.Count > 0)
            {
                var count = 1;
                foreach (var user in userAcceptCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnonymous ? user.EscapedFullName() : "匿名用户")} {user.AcceptCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            sb.AppendLine();
            sb.AppendLine("-- 管理员投稿数量排名 --");
            var adminAcceptCountRank = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.AcceptCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.AcceptCount).Take(topCount).ToListAsync();
            if (adminAcceptCountRank?.Count > 0)
            {
                var count = 1;
                foreach (var user in adminAcceptCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnonymous ? user.EscapedFullName() : "匿名管理员")} {user.AcceptCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            sb.AppendLine();
            sb.AppendLine("-- 管理员审核数量排名 --");
            var adminReviewCountRank = await _userService.Queryable().Where(x => !x.IsBan && !x.IsBot && x.GroupID > 1 && x.ReviewCount > miniumPost && x.ModifyAt >= prev30Days)
                .OrderByDescending(x => x.ReviewCount).Take(topCount).ToListAsync();
            if (adminReviewCountRank?.Count > 0)
            {
                var count = 1;
                foreach (var user in adminReviewCountRank)
                {
                    sb.AppendLine($"{count++}. {(!user.PreferAnonymous ? user.EscapedFullName() : "匿名用户")} {user.ReviewCount}");
                }
            }
            else
            {
                sb.AppendLine("暂无数据");
            }

            await _botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
        }

        /// <summary>
        /// 设置用户组
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("SETUSERGROUP", UserRights.AdminCmd, Alias = "SUGROUP", Description = "设置用户组")]
        public async Task SetUserGroup(Users dbUser, Message message, string[] args)
        {
            async Task<(string, InlineKeyboardMarkup?)> exec()
            {
                var targetUser = await _userService.FetchTargetUser(message);

                if (targetUser == null)
                {
                    if (args.Any())
                    {
                        targetUser = await _userService.FetchUserByUserNameOrUserID(args.First());
                    }
                }

                if (targetUser == null)
                {
                    return ("找不到指定用户", null);
                }

                if (targetUser.IsBan)
                {
                    return ("该用户已被封禁, 无法执行此操作", null);
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return ("无法对自己进行操作", null);
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return ("无法对同级管理员进行此操作", null);
                }

                var keyboard = await _markupHelperService.SetUserGroupKeyboard(dbUser, targetUser);

                return (keyboard != null ? "请选择新的用户组" : "获取可用用户组失败", keyboard);
            }

            (var text, var kbd) = await exec();
            await _botClient.SendCommandReply(text, message, false, replyMarkup: kbd);
        }

        /// <summary>
        /// 设置用户组
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [QueryCmd("SETUSERGROUP", UserRights.AdminCmd)]
        public async Task QResponseSetUserGroup(Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            async Task<string> exec()
            {
                if (args.Length < 3 || !long.TryParse(args[1], out long userId))
                {
                    return "参数有误";
                }

                var targetUser = await _userService.FetchUserByUserID(userId);

                if (targetUser == null)
                {
                    return "找不到指定用户";
                }

                if (targetUser.Id == dbUser.Id)
                {
                    return "无法对自己进行操作";
                }

                if (targetUser.GroupID >= dbUser.GroupID)
                {
                    return "无法对同级管理员进行此操作";
                }

                if (int.TryParse(args[2], out int groupID))
                {
                    var group = _groupRepository.GetGroupById(groupID);
                    if (group != null)
                    {
                        targetUser.GroupID = groupID;
                        targetUser.ModifyAt = DateTime.Now;
                        await _userService.Updateable(targetUser).UpdateColumns(x => new { x.GroupID, x.ModifyAt }).ExecuteCommandAsync();

                        if (targetUser.PrivateChatID != -1)
                        {
                            try
                            {
                                await _botClient.SendTextMessageAsync(targetUser.PrivateChatID, $"您的权限组已被管理员修改为 {group.Name}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError("向用户 {targetUser} 发送消息失败, {ex}", targetUser, ex);
                            }
                        }

                        return $"修改用户 {targetUser} 权限组成功, 当前权限组 {group.Name}";
                    }
                }

                return $"修改用户 {targetUser} 权限组失败, 找不到指定的权限组";
            }

            string text = await exec();
            await _botClient.EditMessageTextAsync(callbackQuery.Message!, text, replyMarkup: null);
        }

        /// <summary>
        /// 查看消息详情
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MESSAGE", UserRights.AdminCmd, Alias = "MSG", Description = "查看消息详情")]
        public async Task GetMessageDetail(Message message)
        {
            var msg = message.ReplyToMessage ?? message;

            StringBuilder sb = new();

            sb.AppendLine("消息详情");

            sb.AppendLine($"Chat Type: <code>{msg.Chat.Type}</code>");
            if (message.Chat.Type != ChatType.Private)
            {
                sb.AppendLine($"Chat Title: <code>{msg.Chat.FullChatProfile()}</code>");
            }
            sb.AppendLine($"Chat Id: <code>{msg.Chat.Id}</code>");

            if (msg.From != null)
            {
                sb.AppendLine($"From: <code>{msg.From.FullName()}</code> <code>{msg.From.UserID()}</code>");
            }
            if (msg.ForwardFrom != null)
            {
                sb.AppendLine($"ForwardFrom: <code>{msg.ForwardFrom.FullName()}</code> <code>{msg.ForwardFrom.UserID()}</code>");
            }

            sb.AppendLine($"Message Id: <code>{msg.MessageId}</code>");

            if (!string.IsNullOrEmpty(msg.MediaGroupId))
            {
                sb.AppendLine($"Media Group Id: <code>{msg.MediaGroupId}</code>");
            }

            sb.AppendLine($"Message Type: <code>{msg.Type}</code>");
            sb.AppendLine($"Message Date: <code>{msg.Date}</code>");

            await _botClient.SendCommandReply(sb.ToString(), message, false, ParseMode.Html);
        }
    }
}
