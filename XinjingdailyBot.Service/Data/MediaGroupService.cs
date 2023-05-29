using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IMediaGroupService), LifeTime.Singleton)]
    [Obsolete("过时表")]
    public sealed class MediaGroupService : BaseService<MediaGroups>, IMediaGroupService
    {
        public async Task AddPostMediaGroup(Message message)
        {
            var postGeoup = new MediaGroups {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                MediaGroupID = message.MediaGroupId!,
                CreateAt = DateTime.Now,
            };

            await InsertAsync(postGeoup);
        }

        public async Task AddPostMediaGroup(IEnumerable<Message> messages)
        {
            var now = DateTime.Now;
            var postGeoups = messages.Select(x => new MediaGroups {
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
    }
}
