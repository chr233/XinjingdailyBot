using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Tasks;

/// <summary>
/// 定期发布稿件处理
/// </summary>
[Job("0 0 0 * * ?")]
internal class PlanedPostsTask : IJob
{
    private readonly ILogger<PlanedPostsTask> _logger;
    private readonly IPostService _postService;
    private readonly IUserService _userService;
    private readonly ITelegramBotClient _botClient;
    private readonly TagRepository _tagRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IChannelService _channelService;
    private readonly IChannelOptionService _channelOptionService;
    private readonly ITextHelperService _textHelperService;
    private readonly IMediaGroupService _mediaGroupService;

    public PlanedPostsTask(
        ILogger<PlanedPostsTask> logger,
        IPostService postService,
        IUserService userService,
        ITelegramBotClient botClient,
        TagRepository tagRepository,
        IAttachmentService attachmentService,
        IChannelService channelService,
        IChannelOptionService channelOptionService,
        ITextHelperService textHelperService,
        IMediaGroupService mediaGroupService)
    {
        _logger = logger;
        _postService = postService;
        _userService = userService;
        _botClient = botClient;
        _tagRepository = tagRepository;
        _attachmentService = attachmentService;
        _channelService = channelService;
        _channelOptionService = channelOptionService;
        _textHelperService = textHelperService;
        _mediaGroupService = mediaGroupService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("开始定时任务, 发布定时任务");

        var post = await _postService.Queryable()
            .Where(static x => x.Status == EPostStatus.InPlan).FirstAsync();

        if (post == null)
        {
            _logger.LogInformation("无延时发布稿件");
            return;
        }

        var poster = await _userService.Queryable().FirstAsync(x => x.UserID == post.PosterUID);
        if (post.IsDirectPost)
        {
            poster.PostCount++;
        }

        ChannelOptions? channel = null;
        if (post.IsFromChannel)
        {
            channel = await _channelOptionService.FetchChannelByChannelId(post.ChannelID);
        }
        string postText = _textHelperService.MakePostText(post, poster, channel);
        bool hasSpoiler = post.HasSpoiler;

        try
        {
            //发布频道发布消息
            if (!post.IsMediaGroup)
            {
                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, warnText, allowSendingWithoutReply: true);
                }

                Message? postMessage = null;
                if (post.PostType == MessageType.Text)
                {
                    postMessage = await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, postText, parseMode: ParseMode.Html, disableWebPagePreview: true);
                }
                else
                {
                    var attachment = await _attachmentService.Queryable().FirstAsync(x => x.PostID == post.Id);

                    var inputFile = new InputFileId(attachment.FileID);
                    var handler = post.PostType switch {
                        MessageType.Photo => _botClient.SendPhotoAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Audio => _botClient.SendAudioAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, title: attachment.FileName),
                        MessageType.Video => _botClient.SendVideoAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        MessageType.Voice => _botClient.SendVoiceAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Document => _botClient.SendDocumentAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html),
                        MessageType.Animation => _botClient.SendAnimationAsync(_channelService.AcceptChannel.Id, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: hasSpoiler),
                        _ => null,
                    };

                    if (handler == null)
                    {
                        _logger.LogError("不支持的稿件类型: {postType}", post.PostType);
                        return;
                    }

                    postMessage = await handler;
                }
                post.PublicMsgID = postMessage?.MessageId ?? -1;
            }
            else
            {
                var attachments = await _attachmentService.Queryable().Where(x => x.PostID == post.Id).ToListAsync();
                var group = new IAlbumInputMedia[attachments.Count];
                for (int i = 0; i < attachments.Count; i++)
                {
                    var attachmentType = attachments[i].Type;
                    if (attachmentType == MessageType.Unknown)
                    {
                        attachmentType = post.PostType;
                    }

                    var inputFile = new InputFileId(attachments[i].FileID);
                    group[i] = attachmentType switch {
                        MessageType.Photo => new InputMediaPhoto(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Audio => new InputMediaAudio(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Video => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = hasSpoiler },
                        MessageType.Voice => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                        MessageType.Document => new InputMediaDocument(inputFile) { Caption = i == attachments.Count - 1 ? postText : null, ParseMode = ParseMode.Html },
                        _ => throw new Exception("未知的稿件类型"),
                    };
                }

                string? warnText = _tagRepository.GetActivedTagWarnings(post.Tags);
                if (!string.IsNullOrEmpty(warnText))
                {
                    await _botClient.SendTextMessageAsync(_channelService.AcceptChannel.Id, warnText, allowSendingWithoutReply: true);
                }

                var postMessages = await _botClient.SendMediaGroupAsync(_channelService.AcceptChannel.Id, group);
                post.PublicMsgID = postMessages.First().MessageId;
                post.PublishMediaGroupID = postMessages.First().MediaGroupId ?? "";

                //记录媒体组消息
                await _mediaGroupService.AddPostMediaGroup(postMessages);
            }
        }
        finally
        {
            post.Status = EPostStatus.Accepted;
            post.ModifyAt = DateTime.Now;

            await _postService.Updateable(post).UpdateColumns(static x => new {
                x.PublicMsgID,
                x.PublishMediaGroupID,
                x.Status,
                x.ModifyAt
            }).ExecuteCommandAsync();
        }
    }
}
