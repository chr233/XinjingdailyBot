using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler;

/// <inheritdoc/>
[AppService(typeof(IMessageHandler), LifeTime.Singleton)]
public sealed class MessageHandler(
        IPostService _postService,
        IGroupMessageHandler _groupMessageHandler,
        ITelegramBotClient _botClient,
        IForwardMessageHandler _forwardMessageHandler) : IMessageHandler
{
    /// <inheritdoc/>
    public async Task OnTextMessageReceived(Users dbUser, Message message)
    {
        if (dbUser.IsBan)
        {
            return;
        }

        if (message.Chat.Type == ChatType.Private)
        {
            if (message.ForwardFromChat != null)
            {
                var handled = await _forwardMessageHandler.OnForwardMessageReceived(dbUser, message).ConfigureAwait(false);
                if (handled)
                {
                    return;
                }
            }
            if (await _postService.CheckPostLimit(dbUser, message, null).ConfigureAwait(false))
            {
                await _postService.HandleTextPosts(dbUser, message).ConfigureAwait(false);
            }
        }
        else if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
        {
            await _groupMessageHandler.OnGroupTextMessageReceived(dbUser, message).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task OnMediaMessageReceived(Users dbUser, Message message)
    {
        if (dbUser.IsBan)
        {
            return;
        }

        if (message.Chat.Type == ChatType.Private)
        {
            if (message.ForwardFromChat != null)
            {
                var handled = await _forwardMessageHandler.OnForwardMessageReceived(dbUser, message).ConfigureAwait(false);
                if (handled)
                {
                    return;
                }
            }
            switch (message.Type)
            {
                case MessageType.Photo:
                case MessageType.Audio:
                case MessageType.Video:
                case MessageType.Voice:
                case MessageType.Document:
                case MessageType.Animation:
                    if (await _postService.CheckPostLimit(dbUser, message, null).ConfigureAwait(false))
                    {
                        if (message.MediaGroupId != null)
                        {
                            await _postService.HandleMediaGroupPosts(dbUser, message).ConfigureAwait(false);
                        }
                        else
                        {
                            await _postService.HandleMediaPosts(dbUser, message).ConfigureAwait(false);
                        }
                    }

                    break;
                default:
                    await _botClient.AutoReplyAsync($"暂不支持的投稿类型 {message.Type}", message).ConfigureAwait(false);
                    break;
            }
        }
    }
}
