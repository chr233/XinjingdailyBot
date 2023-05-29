using Microsoft.Extensions.Logging;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
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
        private readonly INewPostService _postService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IUserService _userService;
        private readonly IMediaGroupService _mediaGroupService;

        public ForwardMessageHandler(
            ILogger<ForwardMessageHandler> logger,
            ITelegramBotClient botClient,
            IChannelService channelService,
            INewPostService postService,
            IMarkupHelperService markupHelperService,
            IUserService userService,
            IMediaGroupService mediaGroupService)
        {
            _logger = logger;
            _botClient = botClient;
            _channelService = channelService;
            _postService = postService;
            _markupHelperService = markupHelperService;
            _userService = userService;
            _mediaGroupService = mediaGroupService;
        }

        public async Task<bool> OnForwardMessageReceived(Users dbUser, Message message)
        {
            if (dbUser.Right.HasFlag(EUserRights.AdminCmd))
            {
                var forwardFrom = message.ForwardFrom!;
                var forwardFromChat = message.ForwardFromChat;
                var foreardMsgId = message.ForwardFromMessageId;

                if (forwardFromChat != null && foreardMsgId != null
                    && (_channelService.IsChannelMessage(forwardFromChat.Id) || _channelService.IsGroupMessage(forwardFromChat.Id)))
                {
                    NewPosts? post = null;

                    bool isMediaGroup = !string.IsNullOrEmpty(message.MediaGroupId);
                    if (!isMediaGroup)
                    {
                        if (forwardFromChat.Id == _channelService.AcceptChannel.Id)
                        {
                            post = await _postService.GetFirstAsync(x => x.PublicMsgID == foreardMsgId);
                        }
                        else if (forwardFromChat.Id == _channelService.ReviewGroup.Id)
                        {
                            post = await _postService.GetFirstAsync(x => x.ReviewMsgID == foreardMsgId || x.ManageMsgID == foreardMsgId);
                        }
                    }
                    else
                    {
                        var groupMsgs = await _mediaGroupService.QueryMediaGroup(message.MediaGroupId);
                        var msgIds = groupMsgs.Select(x => x.MessageID).ToList();

                        if (forwardFromChat.Id == _channelService.AcceptChannel.Id)
                        {
                            post = await _postService.GetFirstAsync(x => msgIds.Contains(x.PublicMsgID));
                        }
                        else if (forwardFromChat.Id == _channelService.ReviewGroup.Id)
                        {
                            post = await _postService.GetFirstAsync(x => msgIds.Contains(x.ReviewMsgID) || msgIds.Contains(x.ManageMsgID));
                        }
                    }

                    if (post != null)
                    {
                        var poster = await _userService.FetchUserByUserID(post.PosterUID);
                        if (poster != null)
                        {
                            if (post.Status == EPostStatus.Reviewing)
                            {
                                await _botClient.AutoReplyAsync("无法操作审核中的稿件", message);
                                return false;
                            }

                            var keyboard = _markupHelperService.QueryPostMenuKeyboard(dbUser, post);

                            string postStatus = post.Status switch
                            {
                                EPostStatus.ConfirmTimeout => "投递超时",
                                EPostStatus.ReviewTimeout => "审核超时",
                                EPostStatus.Rejected => "已拒绝",
                                EPostStatus.Accepted => "已发布",
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
                    }
                }
            }
            return false;
        }
    }
}
