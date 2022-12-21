using XinjingdailyBot.Model.Enums;

namespace XinjingdailyBot.Service.Data
{
    public interface IChannelOptionService
    {
        Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle);
    }
}
