using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command;

/// <summary>
/// 通用命令
/// </summary>
[AppService(LifeTime.Scoped)]
internal class CommonCommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly OptionsSetting _optionsSetting;
    private readonly IBanRecordService _banRecordService;
    private readonly ITextHelperService _textHelperService;
    private readonly ICommandHandler _commandHandler;

    public CommonCommand(
        ITelegramBotClient botClient,
        IOptions<OptionsSetting> options,
        IBanRecordService banRecordService,
        ITextHelperService textHelperService,
        ICommandHandler commandHandler)
    {
        _botClient = botClient;
        _optionsSetting = options.Value;
        _banRecordService = banRecordService;
        _textHelperService = textHelperService;
        _commandHandler = commandHandler;
    }

    /// <summary>
    /// 显示命令帮助
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("HELP", EUserRights.None, Description = "显示命令帮助")]
    public async Task ResponseHelp(Users dbUser, Message message)
    {
        var sb = new StringBuilder();

        if (!dbUser.IsBan)
        {
            sb.AppendLine(_optionsSetting.Message.Help ?? "发送图片/视频或者文字内容即可投稿");
        }
        else
        {
            sb.AppendLine("您已被限制访问此Bot, 仅可使用以下命令: ");
        }
        sb.AppendLine();
        sb.AppendLine(_commandHandler.GetAvilabeCommands(dbUser));

        await _botClient.SendCommandReply(sb.ToString(), message);
    }

    /// <summary>
    /// 首次欢迎语
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("START", EUserRights.None, Description = "首次欢迎语")]
    public async Task ResponseStart(Users dbUser, Message message)
    {
        var sb = new StringBuilder();

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
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("ABOUT", EUserRights.None, Description = "关于机器人")]
    public async Task ResponseAbout(Message message)
    {
        var sb = new StringBuilder();
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
    [TextCmd("VERSION", EUserRights.None, Description = "查看机器人版本")]
    public async Task ResponseVersion(Message message)
    {
        var sb = new StringBuilder();
        string version = Utils.Version;
        string variant = BuildInfo.Variant;
        sb.AppendLine($"程序版本: <code>{version}</code>");
        sb.AppendLine($"子版本: <code>{variant}</code>");
        sb.AppendLine(string.Format("获取开源程序: {0}", _textHelperService.HtmlLink("https://github.com/chr233/XinjingdailyBot/", "XinjingdailyBot")));
        sb.AppendLine(string.Format("爱发电: {0}", _textHelperService.HtmlLink("https://afdian.net/@ylnflp", "@ylnflp")));
        await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
    }

    /// <summary>
    /// 查询自己是否被封禁
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCmd("MYBAN", EUserRights.None, Description = "查询自己是否被封禁")]
    public async Task ResponseMyBan(Users dbUser, Message message)
    {
        var records = await _banRecordService.Queryable()
            .Where(x => x.UserID == dbUser.UserID)
            .OrderByDescending(static x => new { x.BanTime }).ToListAsync();

        var sb = new StringBuilder();

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
                string operate = record.Type switch {
                    EBanType.UnBan => "解封",
                    EBanType.Ban => "封禁",
                    EBanType.Warning => "警告",
                    _ => "其他",
                };
                sb.AppendLine($"在 <code>{date}</code> 因为 <code>{record.Reason}</code> 被 {operate}");
            }
        }

        await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html);
    }
}
