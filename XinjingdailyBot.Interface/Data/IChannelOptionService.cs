using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IChannelOptionService : IBaseService<ChannelOptions>
    {
        Task<ChannelOptions?> FetchChannelByTitle(string channelTitle);
        Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle);
        Task<ChannelOptions?> UpdateChannelOptionById(long channelId, ChannelOption channelOption);
    }
}
