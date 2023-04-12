using System.Text;
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
using XinjingdailyBot.Interface.Helper;
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
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IUserService _userService;

        public ForwardMessageHandler(
            ILogger<ForwardMessageHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService,
            IPostService postService,
           IMarkupHelperService markupHelperService,
           IUserService userService)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
            _postService = postService;
            _markupHelperService = markupHelperService;
            _userService = userService;
        }

        public async Task<bool> OnForwardMessageReceived(Users dbUser, Message message)
        {
            if (!dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                return false;
            }

            var forwardFrom = message.ForwardFrom!;
            var forwardFromChat = message.ForwardFromChat;

            if (forwardFromChat != null
                && (forwardFromChat.Id == _channelService.AcceptChannel.Id || forwardFromChat.Id == _channelService.RejectChannel.Id))
            {
                var post = await _postService.GetFirstAsync(x => x.PublicMsgID == message.ForwardFromMessageId);
                var poster = await _userService.FetchUserByUserID(post.PosterUID);

                if (post != null && poster != null)
                {
                    if (post.Status == PostStatus.Reviewing)
                    {
                        await _botClient.AutoReplyAsync("无法操作审核中的稿件", message);
                        return false;
                    }

                    var keyboard = _markupHelperService.QueryPostMenuKeyboard(dbUser, post);

                    string postStatus = post.Status switch
                    {
                        PostStatus.ConfirmTimeout => "投递超时",
                        PostStatus.ReviewTimeout => "审核超时",
                        PostStatus.Rejected => "已拒绝",
                        PostStatus.Accepted => "已发布",
                        _ => "未知",
                    };
                    string postMode = post.IsDirectPost ? "直接发布" : (post.Anonymous ? "匿名投稿" : "保留来源");
                    string posterLink = poster.HtmlUserLink();

                    StringBuilder sb = new();
                    sb.AppendLine($"投稿人: {posterLink}");
                    sb.AppendLine($"模式: {postMode}");
                    sb.AppendLine($"状态: {postStatus}");

                    await _botClient.SendTextMessageAsync(message.Chat, sb.ToString(), parseMode: ParseMode.Html, disableWebPagePreview: true, replyMarkup: keyboard, replyToMessageId: message.MessageId, allowSendingWithoutReply: true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
