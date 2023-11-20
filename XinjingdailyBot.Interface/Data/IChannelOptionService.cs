using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 频道设定仓储服务
/// </summary>
public interface IChannelOptionService : IBaseService<ChannelOptions>
{
    /// <summary>
    /// 获取频道数量
    /// </summary>
    /// <returns></returns>
    Task<int> ChannelCount();
    /// <summary>
    /// 根据ID获取频道
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    Task<ChannelOptions?> FetchChannelByChannelId(long channelId);
    /// <summary>
    /// 根据UserName获取频道
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="channelTitle"></param>
    /// <returns></returns>
    Task<ChannelOptions?> FetchChannelByNameOrTitle(string channelName, string channelTitle);

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
    /// <param name="chatId"></param>
    /// <param name="chatTitle"></param>
    /// <param name="chatUserName"></param>
    /// <returns></returns>
    Task<EChannelOption> FetchChannelOption(long chatId, string chatTitle, string chatUserName);

    /// <summary>
    /// 更新频道设定
    /// </summary>
    /// <param name="channelId"></param>
    /// <param name="channelOption"></param>
    /// <returns></returns>
    Task<ChannelOptions?> UpdateChannelOptionById(long channelId, EChannelOption channelOption);
}
