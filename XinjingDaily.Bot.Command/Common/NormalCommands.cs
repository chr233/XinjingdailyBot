using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;

namespace XinjingDaily.Bot.Command.Common;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class NormalCommands(
    ITelegramBotService _botClient,
    ILogger<NormalCommands> _logger,
    ICommandHandler _commandHandler)
{
    /// <summary>
    /// 检测机器人是否存活
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    [Permission(ECommandScope.Private)]
    [Permission(ECommandScope.Group, "")]
    [TextCommand("PING", Description = "检测机器人是否存活")]
    public async Task ResponsePing(Message message)
    {
        var now = DateTime.UtcNow;
        var receiveOffset = (now - message.Date).TotalMilliseconds;

        var msg = await _botClient.SendCommandReply("PONG!", message).ConfigureAwait(false);
        var sendOffset = (msg.Date - now).TotalMilliseconds;

        await _botClient.EditMessageText(msg, $"PONG!\r\n收信延时: {receiveOffset:F3}ms\r\n发信延时: {sendOffset:F3}ms").ConfigureAwait(false);
    }
}
