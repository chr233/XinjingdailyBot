using Telegram.Bot.Types;
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
        public async Task<EChannelOption> FetchChannelOption(Chat channelChat)
        {
            var chatId = channelChat.Id;
            var chatTitle = channelChat.Title;
            var chatUserName = channelChat.Username;

            var channel = await Queryable().Where(x => x.ChannelID == chatId).FirstAsync();
            if (channel == null)
            {
                channel = new ChannelOptions {
                    ChannelID = chatId,
                    ChannelName = chatUserName ?? "",
                    ChannelTitle = chatTitle ?? "",
                    Option = EChannelOption.Normal,
                    Count = 1,
                    CreateAt = DateTime.Now,
                    ModifyAt = DateTime.Now,
                };
                await Insertable(channel).ExecuteCommandAsync();
            }
            else
            {
                if (channel.ChannelName != chatUserName || channel.ChannelTitle != chatTitle)
                {
                    channel.ChannelTitle = chatTitle ?? "";
                    channel.ChannelName = chatUserName ?? "";
                    channel.ModifyAt = DateTime.Now;
                }
                channel.Count++;
                await Updateable(channel).ExecuteCommandAsync();
            }

            return channel.Option;
        }

        public async Task<ChannelOptions?> FetchChannelByTitle(string channelTitle)
        {
            var channel = await Queryable().Where(x => x.ChannelTitle == channelTitle).FirstAsync();
            return channel;
        }

        public async Task<ChannelOptions?> FetchChannelByNameOrTitle(string channelName, string channelTitle)
        {
            var channel = await Queryable().Where(x => x.ChannelName == channelName || x.ChannelTitle == channelTitle).FirstAsync();
            return channel;
        }

        public async Task<ChannelOptions?> FetchChannelByChannelId(long channelId)
        {
            if (channelId <= 0)
            {
                return null;
            }
            var channel = await Queryable().Where(x => x.ChannelID == channelId).FirstAsync();
            return channel;
        }

        public async Task<ChannelOptions?> UpdateChannelOptionById(long channelId, EChannelOption channelOption)
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
