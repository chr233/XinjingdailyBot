using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Users;

namespace XinjingDaily.Bot.Interface.Bot.System;

public interface IDispatcherService
{
    Task OnCallbackQueryReceived(UserInfo dbUser, CallbackQuery query);
    Task OnChannalPostReceived(UserInfo dbUser, Message message);
    Task OnInlineQueryReceived(UserInfo dbUser, InlineQuery query);
    Task OnJoinRequestReceived(UserInfo dbUser, ChatJoinRequest request);
    Task OnMessageReceived(UserInfo dbUser, Message message);
    Task OnOtherUpdateReceived(UserInfo dbUser, Update update);
}