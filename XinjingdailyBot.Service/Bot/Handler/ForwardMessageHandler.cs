using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(typeof(IForwardMessageHandler), LifeTime.Singleton)]
    public class ForwardMessageHandler : IForwardMessageHandler
    {
        private readonly ILogger<ForwardMessageHandler> _logger;
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;
        private readonly IPostService _postService;

        public ForwardMessageHandler(
            ILogger<ForwardMessageHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService,
            IPostService postService)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
            _postService = postService;
        }

        /// <summary>
        /// 处理转发的消息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> OnForwardMessageReceived(Users dbUser, Message message)
        {
            var forwardFrom = message.ForwardFrom!;
            var forwardFromChat = message.ForwardFromChat;

            if (forwardFromChat != null && (forwardFromChat.Id == _channelService.AcceptChannel.Id || forwardFromChat.Id == _channelService.RejectChannel.Id))
            {
                var post = await _postService.Queryable().FirstAsync(x => x.PublicMsgID == message.ForwardFromMessageId);
                if (post != null)
                {
                    _logger.LogInformation(post.ToString());

                    await _botClient.AutoReplyAsync("Todo", message);

                    return true;
                }
            }
            return false;
        }
    }
}
