using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Chat;
using XinjingDaily.Bot.Infrastructure;
using XinjingDaily.Bot.Interface.Bot.Storage;
using XinjingDaily.Bot.IRepository.Channel;

namespace XinjingDaily.Bot.Service.Bot.Storage;

[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public sealed class ChatService(
    IOptions<AppSettings> _options,
    IChatInfoRepository _chatInfoRepository) : IChatService
{
    private readonly Dictionary<long, Chat> _channelCache = [];

    public async Task<int> LoadChannelCache()
    {
        //var channels = await _channelRepository.QueryAllChannels().ConfigureAwait(false);
        //foreach (var channel in channels)
        //{
        //    _channelCache[channel.ChatId] = channel;
        //}
        return _channelCache.Count;
    }

    public async Task AutoLeaveChat(Chat chat)
    {
        if (chat == null)
            return;
        if (chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
        {
            //await _botClient.LeaveChatAsync(chat.Id).ConfigureAwait(false);
        }
    }

    public async Task OnNewChatTitle(Chat chat, string? newTitle)
    {
        var chatInfo = await _chatInfoRepository.QueryByTelegramIdAsync(chat.Id).ConfigureAwait(false);
        if (chatInfo != null)
        {

        }
        else
        {
            chatInfo = new ChatInfo {
                TelegramId = chat.Id,
                TelegramName = chat.Username,
                Title = newTitle,
                Type = chat.Type,
                CreateAt = DateTime.Now,
                ModifyAt = DateTime.MinValue
            };

            await _chatInfoRepository.InsertAsync(chatInfo).ConfigureAwait(false);
        }
    }

}
