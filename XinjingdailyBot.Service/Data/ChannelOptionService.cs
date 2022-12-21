using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Service.Bot;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Interface.Helper;

namespace XinjingdailyBot.Service.Data
{
    [AppService(ServiceType = typeof(IPostService), ServiceLifetime = LifeTime.Transient)]
    public sealed class ChannelOptionService : BaseService<ChannelOptions>, IChannelOptionService
    {
        private readonly ILogger<ChannelOptionService> _logger;
        private readonly ChannelOptionRepository _channelOptionRepository;

        public ChannelOptionService(
            ILogger<ChannelOptionService> logger,
            ChannelOptionRepository channelOptionRepository)
        {
            _logger = logger;
            _channelOptionRepository = channelOptionRepository;
        }


        /// <summary>
        /// 获取频道设定
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns></returns>
        public async Task<ChannelOption> FetchChannelOption(long channelId, string? channelName, string? channelTitle)
        {
            var channel = await _channelOptionRepository.Queryable().Where(x => x.ChannelID == channelId).FirstAsync();
            if (channel == null)
            {
                channel = new()
                {
                    ChannelID = channelId,
                    ChannelName = channelName ?? "",
                    ChannelTitle = channelTitle ?? "",
                    Option = ChannelOption.Normal,
                    CreateAt = DateTime.Now,
                    ModifyAt = DateTime.Now,
                };
                await _channelOptionRepository.Insertable(channel).ExecuteCommandAsync();
            }
            else if (channel.ChannelName != channelName || channel.ChannelTitle != channelTitle)
            {
                channel.ChannelTitle = channelTitle ?? "";
                channel.ChannelName = channelName ?? "";
                channel.ModifyAt = DateTime.Now;
                await _channelOptionRepository.Updateable(channel).ExecuteCommandAsync();
            }

            return channel.Option;
        }
    }
}
