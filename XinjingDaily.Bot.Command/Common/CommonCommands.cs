using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Command;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;
using XinjingdailyBot.Service.Helper;

namespace XinjingDaily.Bot.Command.Common;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class CommonCommands(
    ITelegramBotService _botClient,
    ICommandHandler _commandHandler,
    ITextService _textService,
    IOptions<AppSettings> _options)
{
    [TextCommand("CANCEL", "取消当前操作")]
    public async Task CancelCommand(CommandContext? context, Message message)
    {
        await _botClient.AutoReply("已取消当前操作", message).ConfigureAwait(false);
    }

    [TextCommand("HELP", "帮助")]
    public async Task HelpCommand(UserInfo userInfo, Message message)
    {
        var commands = await _commandHandler.GetAvailabeCommands(userInfo, message.Chat.Type).ConfigureAwait(false);

        if (commands.Count == 0)
        {
            await _botClient.AutoReply("没有可用命令", message).ConfigureAwait(false);
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("可用命令列表:");
            foreach (var command in commands)
            {
                if (!string.IsNullOrEmpty(command.Description))
                {
                    sb.AppendLine($" - /{command.Command} : {command.Description}");
                }
                else
                {
                    sb.AppendLine($" - /{command.Command}");
                }
            }
            await _botClient.AutoReply(sb.ToString(), message, Telegram.Bot.Types.Enums.ParseMode.Html).ConfigureAwait(false);
        }
    }

    [TextCommand("START", "关于")]
    public async Task StartCommand(Message message, UserInfo userInfo)
    {
        var sb = new StringBuilder();

        string? msg = "start";// _optionsSetting.Message.Start;
        if (!string.IsNullOrEmpty(msg))
        {
            sb.AppendLine(msg);
        }

        if (!userInfo.IsBan)
        {
            sb.AppendLine("直接发送图片或者文字内容即可投稿");
        }
        else
        {
            sb.AppendLine("您已被限制访问此Bot, 无法使用投稿等功能");
        }

        sb.AppendLine("查看命令帮助: /help");
        await _botClient.SendCommandReply(sb.ToString(), message).ConfigureAwait(false);
    }

    [TextCommand("ABOUT", "关于")]
    public async Task AboutCommand(Message message)
    {
        var sb = new StringBuilder();
        string? msg = "about"; //_options.Value.Message.About;
        if (!string.IsNullOrEmpty(msg))
        {
            sb.AppendLine(msg);
        }
        sb.AppendLine("Powered by @xinjingdaily");
        await _botClient.SendCommandReply(sb.ToString(), message).ConfigureAwait(false);
    }

    /// <summary>
    /// 查看机器人版本
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [TextCommand("VERSION", "查看机器人版本")]
    public async Task ResponseVersion(Message message)
    {
        var sb = new StringBuilder();
        string version = BuildInfo.Version ?? "null";
        string variant = BuildInfo.Variant;
        sb.AppendLine($"程序版本: <code>{version}</code>");
        sb.AppendLine($"子版本: <code>{variant}</code>");
        sb.AppendLine(string.Format("获取开源程序: {0}", _textService.HtmlLink("https://github.com/chr233/XinjingdailyBot/", "XinjingdailyBot")));
        sb.AppendLine(string.Format("爱发电: {0}", _textService.HtmlLink("https://afdian.com/a/ylnflp", "@ylnflp")));
        await _botClient.SendCommandReply(sb.ToString(), message, parsemode: ParseMode.Html).ConfigureAwait(false);
    }
}
