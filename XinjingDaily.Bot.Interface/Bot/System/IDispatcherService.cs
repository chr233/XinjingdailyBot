using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Users;

namespace XinjingDaily.Bot.Interface.Bot.System;

public interface IDispatcherService
{
    Task OnCallbackQueryReceived(UserInfos dbUser, CallbackQuery query);
    Task OnChannalPostReceived(UserInfos dbUser, Message message);
    Task OnInlineQueryReceived(UserInfos dbUser, InlineQuery query);
    Task OnJoinRequestReceived(UserInfos dbUser, ChatJoinRequest request);
    Task OnMessageReceived(UserInfos dbUser, Message message);
    Task OnOtherUpdateReceived(UserInfos dbUser, Update update);
}