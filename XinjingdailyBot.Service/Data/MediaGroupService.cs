using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IMediaGroupService), LifeTime.Singleton)]
    public sealed class MediaGroupService : BaseService<MediaGroups>, IMediaGroupService
    {
        public async Task AddPostMediaGroup(IEnumerable<Message> messages)
        {
            var now = DateTime.Now;
            var postGeoups = messages.Select(x => new MediaGroups
            {
                ChatID = x.Chat.Id,
                PublicMsgID = x.MessageId,
                MediaGroupID = x.MediaGroupId!,
                CreateAt = now,
            }).ToList();

            await Storageable(postGeoups).ExecuteCommandAsync();
        }
    }
}
