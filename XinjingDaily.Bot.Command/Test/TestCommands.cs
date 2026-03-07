using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;
using XinjingDaily.Bot.Interface.Bot.Storage;

namespace XinjingDaily.Bot.Command.Test;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class TestCommands(
    ITelegramBotService _botClient,
    IUserService _userService,
    ICommandHandler _commandHandler)
{
    [TextCommand("A", "测试命令 - 默认权限")]
    public async Task ACommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, null)]
    [TextCommand("P", "测试指令 - 私聊命令")]
    public async Task PCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Group, null)]
    [TextCommand("G", "测试指令 - 群组命令")]
    public async Task GCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, null)]
    [TextCommand("TA", "测试指令 - 全部场景")]
    public async Task TestCommand(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.Private, "test:1")]
    [Permission(ECommandScope.Group, "test:2")]
    [TextCommand("TP", "测试指令 - 权限分级")]
    public async Task Test1Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [Permission(ECommandScope.All, "test:3")]
    [TextCommand("TT", "测试指令")]
    public async Task Test2Command(UserInfo userInfo, Message message)
    {
        await _botClient.AutoReply(userInfo.ToString(), message).ConfigureAwait(false);
    }

    [TextCommand("PERMISSION", "获取当前用户的权限")]
    public async Task GetMyPermission(UserInfo userInfo, Message message)
    {
        var claims = await _userService.QueryUserClaims(userInfo).ConfigureAwait(false);

        if (claims == null || claims.Count == 0)
        {
            await _botClient.AutoReply("你没有任何权限", message).ConfigureAwait(false);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("你拥有以下权限:");
        foreach (var claim in claims)
        {
            sb.AppendLine($" - <code>{claim}</code>");
        }

        await _botClient.AutoReply(sb.ToString(), message, Telegram.Bot.Types.Enums.ParseMode.Html).ConfigureAwait(false);
    }

    [TextCommand("ClearCommands", "清空命令菜单")]
    public async Task ClearCommands(UserInfo userInfo, Message message)
    {
        await _commandHandler.ClearCommandsMenu().ConfigureAwait(false);
        await _botClient.AutoReply("已清除所有命令", message).ConfigureAwait(false);
    }

    [TextCommand("TestCallback")]
    public async Task TestCallback(UserInfo userInfo, Message message, string[] args)
    {
        if (args.Length == 0)
        {
            await _botClient.AutoReply("请提供回调数据", message).ConfigureAwait(false);
            return;
        }

        var markup = new InlineKeyboardMarkup {
            InlineKeyboard = args
                .Chunk(5)
                .Select(static row => row.Select(static x => new InlineKeyboardButton(x, x)))
        };

        await _botClient.SendMessage(message, "点击下面的按钮触发回调", replyMarkup: markup).ConfigureAwait(false);
    }
}
