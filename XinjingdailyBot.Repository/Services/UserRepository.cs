using SqlSugar;
using StackExchange.Redis;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository.Base;

namespace XinjingdailyBot.Repository.Services;

/// <summary>
/// 稿件仓储类
/// </summary>
[AppService(LifeTime.Transient)]
public class UserRepository(
    ISqlSugarClient _context, 
    IConnectionMultiplexer connectionMultiplexer) : BaseRepository<Users>(_context)
{
    public async Task<Users?> GetOrCreateUser()
    {
        return null;
    }

    /// <inheritdoc/>
    public async Task<Users?> FetchUserFromUpdate(Update update)
    {
        //var msgChat = update.Type switch {
        //    UpdateType.ChannelPost => update.ChannelPost!.Chat,
        //    UpdateType.EditedChannelPost => update.EditedChannelPost!.Chat,
        //    UpdateType.Message => update.Message!.Chat,
        //    UpdateType.EditedMessage => update.EditedMessage!.Chat,
        //    UpdateType.ChatJoinRequest => update.ChatJoinRequest!.Chat,
        //    _ => null
        //};

        //await AutoLeaveChat(msgChat).ConfigureAwait(false);

        //var message = update.Type switch {
        //    UpdateType.Message => update.Message!,
        //    UpdateType.ChannelPost => update.ChannelPost!,
        //    _ => null,
        //};

        //// 自动删除置顶通知 和 群名修改通知
        //if (message != null && (message.Type == MessageType.MessagePinned || message.Type == MessageType.ChatTitleChanged))
        //{
        //    await AutoDeleteNotification(message).ConfigureAwait(false);
        //    return null;
        //}

        //if (update.Type == UpdateType.ChannelPost)
        //{
        //    return await QueryUserFromChannelPost(update.ChannelPost!).ConfigureAwait(false);
        //}
        //else
        //{
        //    var msgUser = update.Type switch {
        //        UpdateType.ChannelPost => update.ChannelPost!.From,
        //        UpdateType.EditedChannelPost => update.EditedChannelPost!.From,
        //        UpdateType.Message => update.Message!.From,
        //        UpdateType.EditedMessage => update.EditedMessage!.From,
        //        UpdateType.CallbackQuery => update.CallbackQuery!.From,
        //        UpdateType.InlineQuery => update.InlineQuery!.From,
        //        UpdateType.ChosenInlineResult => update.ChosenInlineResult!.From,
        //        UpdateType.ChatJoinRequest => update.ChatJoinRequest!.From,
        //        _ => null
        //    };

        //    return await QueryUserFromChat(msgUser, msgChat).ConfigureAwait(false);
        //}

        return null;
    }
}
