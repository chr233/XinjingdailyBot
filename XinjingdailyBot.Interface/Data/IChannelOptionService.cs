using XinjingdailyBot.Infrastructure.Enums;

namespace XinjingdailyBot.Interface.Data
{
    public interface IChannelOptionService
    {
        Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle);
    }
}
