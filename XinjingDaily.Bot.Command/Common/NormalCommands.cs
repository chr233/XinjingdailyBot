using System.Text;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Command;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;
using XinjingDaily.Bot.Interface.Bot;
using XinjingDaily.Bot.Interface.Bot.Handler;

namespace XinjingDaily.Bot.Command.Common;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class NormalCommands(
    ITelegramBotService _botClient,
    ICommandHandler _commandHandler)
{
    [TextCommand("CANCEL", "取消当前操作")]
    public async Task CancelCommand(CommandContext? context, Message message)
    {
        await _botClient.AutoReply("已取消当前操作", message).ConfigureAwait(false);
    }

    [Permission(EPermission.PostDeleteOwn)]
    [TextCommand("Help", "帮助")]
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
}
