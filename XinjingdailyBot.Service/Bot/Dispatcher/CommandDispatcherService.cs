using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Interface.Bot.Dispatcher;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Dispatcher
{
    [AppService(ServiceType = typeof(ICommandDispatcherService), ServiceLifetime = LifeTime.Scoped)]
    public class CommandDispatcherService : ICommandDispatcherService
    {
        private readonly ILogger<CommandDispatcherService> _logger;
        private readonly OptionsSetting _optionsSetting;
        private readonly ITelegramBotClient _botClient;
        private readonly ITextHelperService _userService;
        private readonly IChannelService _channelService;
        private readonly ITextHelperService _textHelperService;
        private readonly IPostService _postService;
        private readonly ICmdRecordService _cmdRecordService;

        public CommandDispatcherService(
            ILogger<CommandDispatcherService> logger,
            IOptions<OptionsSetting> optionsSetting,
            ITelegramBotClient botClient,
            ITextHelperService userService,
            IChannelService channelService,
            ITextHelperService textHelperService,
            IPostService postService,
            ICmdRecordService cmdRecordService)
        {
            _logger = logger;
            _optionsSetting = optionsSetting.Value;
            _botClient = botClient;
            _userService = userService;
            _channelService = channelService;
            _textHelperService = textHelperService;
            _postService = postService;
            _cmdRecordService = cmdRecordService;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnTextCommandReceived(Users dbUser, Message message)
        {
            bool needRecord = true;
            bool handled = false;
            string? exception = null;

            try
            {
                (needRecord, handled, bool autoDelete) = await ExecTextCommand(dbUser, message);

                //定时删除命令消息
                if (autoDelete)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        try
                        {
                            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                        catch
                        {
                            _logger.LogError("删除消息 {messageId} 失败", message.MessageId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                exception = $"{ex.GetType} {ex.Message}";
                throw;
            }
            finally
            {
                if (needRecord)
                {
                    string command = message.Text![1..];
                    await _cmdRecordService.AddCmdRecord(message, dbUser, command, handled, false, exception);
                }
            }
        }

        public async Task OnQueryCommandReceived(Users dbUser, CallbackQuery callbackQuery, string[] args)
        {
            Message message = callbackQuery.Message!;

            if (args.Length < 2 || !long.TryParse(args[0], out long userID))
            {

                await _botClient.AutoReplyAsync("Payload 非法", callbackQuery);
                await _botClient.RemoveMessageReplyMarkupAsync(message);
                return;
            }

            //判断消息发起人是不是同一个
            if (dbUser.UserID != userID)
            {
                await _botClient.AutoReplyAsync("这不是你的消息, 请不要瞎点", callbackQuery);
                return;
            }

            bool handled = false;
            string? exception = null;

            try
            {
                (handled, bool autoDelete) = await ExecQueryCommand(dbUser, callbackQuery, message, args[1..]);

                //定时删除命令消息
                if (autoDelete)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        try
                        {
                            await _botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                        catch
                        {
                            _logger.LogError("删除消息 {messageId} 失败", message.MessageId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                exception = $"{ex.GetType} {ex.Message}";
                throw;
            }
            finally
            {
                string command = callbackQuery.Data!;
                await _cmdRecordService.AddCmdRecord(message, dbUser, command, handled, true, exception);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message">用户消息原文</param>
        /// <returns>needRecord,handled,autoDelete</returns>
        private async Task<(bool, bool, bool)> ExecTextCommand(Users dbUser, Message message)
        {
            //切分命令参数
            string[] args = message.Text!.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            if (!args.Any()) { return (false, false, false); }

            string cmd = args.First()[1..];
            args = args[1..];

            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            //判断是不是艾特机器人的命令
            bool isAtBot = false;
            int index = cmd.IndexOf('@');
            if (inGroup && index != -1)
            {
                string botName = cmd[(index + 1)..];
                if (botName.Equals(_channelService.BotUser.Username, StringComparison.OrdinalIgnoreCase))
                {
                    isAtBot = true;
                    cmd = cmd[..index];
                }
                else
                {
                    return (false, false, false);
                }
            }

            //检查权限
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;
            bool reviewPost = dbUser.Right.HasFlag(UserRights.ReviewPost);

            //是否自动删除消息
            bool autoDelete = true;
            //是否成功响应命令
            bool handled = true;
            switch (cmd.ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权
                case "HELP":
                    await CommonCmd.ResponseHelp(dbUser, message);
                    break;

                case "START":
                    await CommonCmd.ResponseStart(dbUser, message);
                    break;

                case "VERSION":
                    await CommonCmd.ResponseVersion(message);
                    break;

                case "MYBAN":
                    await CommonCmd.ResponseMyBan(dbUser, message);
                    break;

                //Normal - 普通命令
                case "PING" when normal:
                    await NormalCmd.ResponsePing(message);
                    break;

                case "ANONYMOUS" when normal:
                    await NormalCmd.ResponseAnymouse(dbUser, message);
                    break;

                case "NOTIFICATE" when normal:
                case "NOTIFICATION" when normal:
                    await NormalCmd.ResponseNotification(dbUser, message);
                    break;

                case "MYINFO" when normal:
                    await NormalCmd.ResponseMyInfo(dbUser, message);
                    break;

                case "MYRIGHT" when normal:
                    await NormalCmd.ResponseMyRight(dbUser, message);
                    break;

                case "ADMIN" when normal:
                case "ADMINS" when normal:
                    await NormalCmd.ResponseCallAdmins(message);
                    break;

                //Admin - 管理员命令
                case "GROUPINFO" when admin:
                    await AdminCmd.ResponseGroupInfo(message);
                    break;

                case "INFO" when admin:
                case "UINFO" when admin:
                case "USERINFO" when admin:
                    await AdminCmd.ResponseUserInfo(message, args);
                    break;

                case "BAN" when admin:
                    await AdminCmd.ResponseBan(dbUser, message, args);
                    autoDelete = false;
                    break;

                case "UNBAN" when admin:
                    await AdminCmd.ResponseUnban(dbUser, message, args);
                    autoDelete = false;
                    break;

                case "WARN" when admin:
                case "WARNING" when admin:
                    await AdminCmd.ResponseWarning(dbUser, message, args);
                    autoDelete = false;
                    break;

                case "QBAN" when admin:
                case "QUERYBAN" when admin:
                    await AdminCmd.ResponseQueryBan(message, args);
                    break;

                case "ECHO" when admin:
                    await AdminCmd.ResponseEcho(dbUser, message, args);
                    autoDelete = false;
                    break;

                case "QUSER" when admin:
                case "QUERYUSER" when admin:
                case "SEARCHUSER" when admin:
                    await AdminCmd.ResponseSearchUser(dbUser, message, args);
                    break;

                case "POSTREPORT" when admin:
                    await AdminCmd.ResponsePostReport(message);
                    break;

                case "SYSREPORT" when admin:
                    await AdminCmd.ResponseSystemReport(message);
                    break;

                case "INVITE" when admin:
                    await AdminCmd.ResponseInviteToReviewGroup(dbUser, message);
                    break;

                case "URANK" when admin:
                case "USERRANK" when admin:
                    await AdminCmd.ResponseUserRank(message);
                    break;

                //Super - 超级管理员命令
                case "RESTART" when super:
                    await SuperCmd.ResponseRestart(message);
                    break;

                case "SETUSERGROUP" when super:
                    await SuperCmd.SetUserGroup(dbUser, message, args);
                    break;

                //Review - 审核命令
                case "NO" when reviewPost:
                    await ReviewCmd.ResponseNo(dbUser, message, args);
                    autoDelete = false;
                    break;

                case "EDIT" when reviewPost:
                    await ReviewCmd.ResponseEditPost(dbUser, message, args);
                    autoDelete = false;
                    break;

                default:
                    //仅在私聊,或者艾特机器人时提示未知命令
                    if (isAtBot || !inGroup)
                    {
                        await _botClient.SendCommandReply("未知命令, 获取帮助 /help", message, false);
                    }
                    handled = false;
                    break;
            }

            //自动删除命令的时机
            //1.autoDelete = true
            //2.在群组中
            //3.成功执行命令
            return (true, handled, autoDelete && inGroup && handled);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message">用户消息原文</param>
        /// <returns>handled,autoDelete</returns>
        private async Task<(bool, bool)> ExecQueryCommand(Users dbUser, CallbackQuery callbackQuery, Message message, string[] args)
        {
            bool inGroup = message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup;

            //检查权限
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;

            //是否自动删除消息
            bool autoDelete = true;
            //是否成功响应命令
            bool handled = true;
            switch (args[0].ToUpperInvariant())
            {
                //Common - 通用命令, 不鉴权

                //Normal - 普通命令
                case "SAY" when normal:
                    await NormalCmd.ResponseSay(callbackQuery, args);
                    break;

                case "CANCEL" when normal:
                    await NormalCmd.ResponseCancel(callbackQuery, false, args);
                    break;

                case "CANCELCLOSE" when normal:
                case "CANCELANDCLOSE" when normal:
                    await NormalCmd.ResponseCancel(callbackQuery, true, args);
                    break;

                //Admin - 管理员命令
                case "QUERYUSER" when admin:
                case "SEARCHUSER" when admin:
                    await AdminCmd.ResponseSearchUser(dbUser, callbackQuery, args);
                    autoDelete = false;
                    break;

                //Super - 超级管理员命令
                case "SETUSERGROUP" when super:
                    await SuperCmd.ResponseSetUserGroup(dbUser, callbackQuery, args);
                    autoDelete = false;
                    break;

                default:
                    //提示未处理的命令
                    await _botClient.AutoReplyAsync("未知命令, 获取帮助 /help", message);
                    handled = false;
                    break;
            }

            //自动删除命令的时机
            //1.autoDelete = true
            //2.在群组中
            //3.成功执行命令
            return (handled, autoDelete && inGroup && handled);
        }

    }
}
