using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IChannelOptionService : IBaseService<ChannelOptions>
    {
        /// <summary>
        /// 通过频道名称获取频道ID
        /// </summary>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        Task<ChannelOptions?> FetchChannelByTitle(string channelTitle);
        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelChat"></param>
        /// <returns></returns>
        Task<EChannelOption> FetchChannelOption(Chat channelChat);
        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        [Obsolete("过时的方法")]
        Task<EChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle);
        /// <summary>
        /// 更新频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelOption"></param>
        /// <returns></returns>
        Task<ChannelOptions?> UpdateChannelOptionById(long channelId, EChannelOption channelOption);
    }
}
