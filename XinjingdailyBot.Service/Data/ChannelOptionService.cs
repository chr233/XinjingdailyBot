using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IChannelOptionService), LifeTime.Transient)]
    public sealed class ChannelOptionService : BaseService<ChannelOptions>, IChannelOptionService
    {
        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        public async Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle)
        {
            var channel = await Queryable().Where(x => x.ChannelID == channelId).FirstAsync();
            if (channel == null)
            {
                channel = new()
                {
                    ChannelID = channelId,
                    ChannelName = channelName ?? "",
                    ChannelTitle = channelTitle ?? "",
                    Option = ChannelOption.Normal,
                    Count = 1,
                    CreateAt = DateTime.Now,
                    ModifyAt = DateTime.Now,
                };
                await Insertable(channel).ExecuteCommandAsync();
            }
            else
            {
                if (channel.ChannelName != channelName || channel.ChannelTitle != channelTitle)
                {
                    channel.ChannelTitle = channelTitle ?? "";
                    channel.ChannelName = channelName ?? "";
                    channel.ModifyAt = DateTime.Now;
                }
                channel.Count++;
                await Updateable(channel).ExecuteCommandAsync();
            }

            return channel.Option;
        }

        /// <summary>
        /// 通过频道名称获取频道ID
        /// </summary>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        public async Task<ChannelOptions?> FetchChannelByTitle(string channelTitle)
        {
            var channel = await Queryable().Where(x => x.ChannelTitle == channelTitle).FirstAsync();
            return channel;
        }

        /// <summary>
        /// 更新频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelOption"></param>
        /// <returns></returns>
        public async Task<ChannelOptions?> UpdateChannelOptionById(long channelId, ChannelOption channelOption)
        {
            var channel = await Queryable().Where(x => x.ChannelID == channelId).FirstAsync();
            if (channel != null)
            {
                channel.Option = channelOption;
                channel.ModifyAt = DateTime.Now;
                await Updateable(channel).ExecuteCommandAsync();
            }
            return channel;
        }
    }
}
