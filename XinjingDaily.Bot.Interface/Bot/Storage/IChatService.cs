using Telegram.Bot.Types;

namespace XinjingDaily.Bot.Interface.Bot.Storage;

public interface IChatService
{
    Task AutoLeaveChat(Chat chat);
    Task LoadChannelCache();
    Task OnNewChatTitle(Chat chat, string? newTitle);
}