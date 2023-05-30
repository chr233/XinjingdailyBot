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
        private readonly IChannelService _channelService;
        private readonly ITelegramBotClient _botClient;
        private readonly IPostService _postService;
        private readonly IMarkupHelperService _markupHelperService;
        private readonly IUserService _userService;
        private readonly IMediaGroupService _mediaGroupService;

        public ForwardMessageHandler(
            ITelegramBotClient botClient,
            IChannelService channelService,
            IPostService postService,
            IMarkupHelperService markupHelperService,
            IUserService userService,
            IMediaGroupService mediaGroupService)
        {
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
                var forwardChatId = message.ForwardFromChat?.Id ?? -1;
                var foreardMsgId = message.ForwardFromMessageId ?? -1;

                if (forwardChatId != -1 && foreardMsgId != -1 &&
                   (_channelService.IsChannelMessage(forwardChatId) || _channelService.IsGroupMessage(forwardChatId)))
                {
                    NewPosts? post = null;

                    var mediaGroup = await _mediaGroupService.QueryMediaGroup(forwardChatId, foreardMsgId);
                    if (mediaGroup == null)
                    {
                        if (_channelService.IsChannelMessage(forwardChatId)) //转发自发布频道或拒绝存档
                        {
                            post = await _postService.GetFirstAsync(x => x.PublicMsgID == foreardMsgId);
                        }
                        else //转发自关联群组
                        {
                            post = await _postService.GetFirstAsync(x =>
                                (x.ReviewChatID == forwardChatId && x.ReviewMsgID == foreardMsgId) ||
                                (x.ReviewActionChatID == forwardChatId && x.ReviewActionMsgID == foreardMsgId)
                            );
                        }
                    }
                    else
                    {
                        if (_channelService.IsChannelMessage(forwardChatId)) //转发自发布频道或拒绝存档
                        {
                            post = await _postService.GetFirstAsync(x => x.PublishMediaGroupID == mediaGroup.MediaGroupID);
                        }
                        else //转发自关联群组 (仅支持审核群)
                        {
                            post = await _postService.GetFirstAsync(x => x.ReviewMediaGroupID == mediaGroup.MediaGroupID);
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

                            string postStatus = post.Status switch {
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
