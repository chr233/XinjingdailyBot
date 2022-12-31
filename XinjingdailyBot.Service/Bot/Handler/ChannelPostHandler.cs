using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(ServiceType = typeof(IChannelPostHandler), ServiceLifetime = LifeTime.Scoped)]
    public class ChannelPostHandler : IChannelPostHandler
    {
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;

        public ChannelPostHandler(
            ITelegramBotClient botClient,
            IChannelService channelService)
        {
            _botClient = botClient;
            _channelService = channelService;
        }

        public async Task OnTextChannelPost(Users dbUser, Message message)
        {

        }
        public async Task OnMediaChannelPost(Users dbUser, Message message)
        {

        }
        public async Task OnMediaGroupChannelPost(Users dbUser, Message message)
        {

        }
    }
}
