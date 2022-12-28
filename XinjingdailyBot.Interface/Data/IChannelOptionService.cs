using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IChannelOptionService : IBaseService<ChannelOptions>
    {
        Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle);
    }
}
