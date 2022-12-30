using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(ServiceType = typeof(IMessageHandler), ServiceLifetime = LifeTime.Scoped)]
    public class MessageHandler : IMessageHandler
    {
        private readonly ILogger<MessageHandler> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IChannelService _channelService;
        private readonly ITextHelperService _textHelperService;
        private readonly IPostService _postService;
        private readonly OptionsSetting _optionsSetting;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService,
            ITextHelperService textHelperService,
            IPostService postService,
            IOptions<OptionsSetting> optionsSetting)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
            _textHelperService = textHelperService;
            _postService = postService;
            _optionsSetting = optionsSetting.Value;
        }

        /// <summary>
        /// 处理文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnTextMessageReceived(Users dbUser, Message message)
        {
            if (dbUser.IsBan)
            {
                return;
            }

            if (message.Chat.Type == ChatType.Private)
            {
                await _postService.HandleTextPosts(dbUser, message);
            }
        }

        /// <summary>
        /// 处理非文本消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnMediaMessageReceived(Users dbUser, Message message)
        {
            if (dbUser.IsBan)
            {
                return;
            }

            if (message.Chat.Type == ChatType.Private)
            {
                if (message.MediaGroupId != null)
                {
                    await _postService.HandleMediaGroupPosts(dbUser, message);
                }
                else
                {
                    await _postService.HandleMediaPosts(dbUser, message);
                }
            }
        }
    }
}
