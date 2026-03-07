using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.Infrastructure.Attribute;
using XinjingDaily.Bot.Infrastructure.Enums;

namespace XinjingDaily.Bot.Interface.Bot.Handler;

public interface ICommandHandler
{
    Task<bool> ClearCommandsMenu();
    Task<List<BotCommand>> GetAvailabeCommands(UserInfo userInfo, ChatType chatType);
    Task OnCommandReceived(UserInfo userInfo, Message message);
    Task OnQueryCommandReceived(UserInfo dbUser, CallbackQuery query);
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, QueryCommandAttribute attribute);
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, QueryCommandAttribute attribute);
    void RegisterQueryCommand(Type classType, MethodInfo methodInfo, QueryCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, PermissionAttribute permission, TextCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, ECommandScope scope, string? permission, TextCommandAttribute attribute);
    void RegisterTextCommand(Type classType, MethodInfo methodInfo, TextCommandAttribute attribute);
    Task<bool> SetCommandsMenu();
}