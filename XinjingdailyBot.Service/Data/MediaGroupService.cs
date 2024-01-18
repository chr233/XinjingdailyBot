using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="IMediaGroupService"/>
[AppService(typeof(IMediaGroupService), LifeTime.Singleton)]
public sealed class MediaGroupService : BaseService<MediaGroups>, IMediaGroupService
{
    public MediaGroupService(ISqlSugarClient context) : base(context)
    {
    }

    public async Task AddPostMediaGroup(Message message)
    {
        if (string.IsNullOrEmpty(message.MediaGroupId))
        {
            return;
        }

        var postGeoup = new MediaGroups {
            ChatID = message.Chat.Id,
            MessageID = message.MessageId,
            MediaGroupID = message.MediaGroupId!,
            CreateAt = DateTime.Now,
        };

        await Insertable(postGeoup).ExecuteCommandAsync();
    }

    public async Task AddPostMediaGroup(IEnumerable<Message> messages)
    {
        if (messages.Any(x => string.IsNullOrEmpty(x.MediaGroupId)))
        {
            return;
        }

        var now = DateTime.Now;
        var postGeoups = messages.Where(x => !string.IsNullOrEmpty(x.MediaGroupId)).Select(x => new MediaGroups {
            ChatID = x.Chat.Id,
            MessageID = x.MessageId,
            MediaGroupID = x.MediaGroupId!,
            CreateAt = now,
        }).ToList();

        await Storageable(postGeoups).ExecuteCommandAsync();
    }

    public async Task<MediaGroups?> QueryMediaGroup(Message message)
    {
        var mediaGroup = await Queryable().FirstAsync(x => x.ChatID == message.Chat.Id && x.MessageID == message.MessageId);
        return mediaGroup;
    }

    public async Task<List<MediaGroups>> QueryMediaGroup(string? mediaGroupId)
    {
        var mediaGroups = await Queryable().Where(x => x.MediaGroupID == mediaGroupId).ToListAsync();
        return mediaGroups;
    }

    public async Task<MediaGroups?> QueryMediaGroup(Chat chat, long msgId)
    {
        var mediaGroup = await Queryable().FirstAsync(x => x.ChatID == chat.Id && x.MessageID == msgId);
        return mediaGroup;
    }

    public async Task<MediaGroups?> QueryMediaGroup(long chatId, int msgId)
    {
        var mediaGroup = await Queryable().FirstAsync(x => x.ChatID == chatId && x.MessageID == msgId);
        return mediaGroup;
    }
}
