using System;
using System.Reflection;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using SqlSugar.IOC;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.Service.Data;

namespace XinjingdailyBot.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class CommonCommand
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly LevelRepository _levelRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OptionsSetting _optionsSetting;
        private readonly IBanRecordService _banRecordService;
        private readonly ITextHelperService _textHelperService;

        public CommonCommand(
            ITelegramBotClient botClient,
            IUserService userService,
            LevelRepository levelRepository,
            GroupRepository groupRepository,
            IOptions<OptionsSetting> options,
            IBanRecordService banRecordService,
            ITextHelperService textHelperService)
        {
            //_logger = logger;
            _botClient = botClient;
            _userService = userService;
            _levelRepository = levelRepository;
            _groupRepository = groupRepository;
            _optionsSetting = options.Value;
            _banRecordService = banRecordService;
            _textHelperService = textHelperService;
        }

        /// <summary>
        /// 显示命令帮助
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("HELP", UserRights.None, Description = "显示命令帮助")]
        public async Task ResponseHelp(Users dbUser, Message message)
        {
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd) || super;
            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd) || admin;
            bool review = dbUser.Right.HasFlag(UserRights.ReviewPost);

            StringBuilder sb = new();

            if (!dbUser.IsBan)
            {
                sb.AppendLine("发送图片/视频或者文字内容即可投稿");
            }
            else
            {
                sb.AppendLine("您已被限制访问此Bot, 仅可使用以下命令: \n");
            }

            await _botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 首次欢迎语
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("START", UserRights.None, Description = "首次欢迎语")]
        public async Task ResponseStart(Users dbUser, Message message)
        {
            StringBuilder sb = new();

            string? msg = _optionsSetting.Message.Start;
            if (!string.IsNullOrEmpty(msg))
            {
                sb.AppendLine(msg);
            }

            if (!dbUser.IsBan)
            {
                sb.AppendLine("直接发送图片或者文字内容即可投稿");
            }
            else
            {
                sb.AppendLine("您已被限制访问此Bot, 无法使用投稿等功能");
            }

            sb.AppendLine("查看命令帮助: /help");
            await _botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 关于机器人
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("ABOUT", UserRights.None, Description = "关于机器人")]
        public async Task ResponseAbout(Users dbUser, Message message)
        {
            StringBuilder sb = new();
            string? msg = _optionsSetting.Message.About;
            if (!string.IsNullOrEmpty(msg))
            {
                sb.AppendLine(msg);
            }
            sb.AppendLine("Powered by @xinjingdaily");
            await _botClient.SendCommandReply(sb.ToString(), message);
        }

        /// <summary>
        /// 查看机器人版本
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("VERSION", UserRights.None, Description = "查看机器人版本")]
        public async Task ResponseVersion(Message message)
        {
            StringBuilder sb = new();
            var version = Assembly.Load("XinjingdailyBot.WebAPI").GetName().Version;
            sb.AppendLine($"当前机器人版本: <code>{version}</code>");
            sb.Append("获取开源程序: ");
            sb.AppendLine(_textHelperService.HtmlLink("https://github.com/chr233/XinjingdailyBot/", "Xinjingdaily"));
            await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }

        /// <summary>
        /// 查询自己是否被封禁
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("MYBAN", UserRights.None, Description = "查询自己是否被封禁")]
        public async Task ResponseMyBan(Users dbUser, Message message)
        {
            var records = await _banRecordService.Queryable().Where(x => x.UserID == dbUser.UserID).ToListAsync();

            StringBuilder sb = new();

            string status = dbUser.IsBan ? "已封禁" : "正常";
            sb.AppendLine($"用户名: <code>{dbUser.EscapedFullName()}</code>");
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
                    string operate = record.Type switch
                    {
                        BanType.UnBan => "解封",
                        BanType.Ban => "封禁",
                        BanType.Warning => "警告",
                        _ => "其他",
                    };
                    sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> 被 {operate}");
                }
            }

            await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
        }
    }
}
