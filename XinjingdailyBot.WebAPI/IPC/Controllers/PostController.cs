using Microsoft.AspNetCore.Mvc;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;
using XinjingdailyBot.WebAPI.IPC.Requests;
using XinjingdailyBot.WebAPI.IPC.Responses;

namespace XinjingdailyBot.WebAPI.IPC.Controllers;

/// <summary>
/// 主页控制器
/// </summary>
[Route("Api/[controller]", Name = "投稿")]
public sealed class PostController : XjbController
{
    private readonly ILogger<PostController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPostService _postService;
    private readonly IChannelOptionService _channelOptionService;
    private readonly IUserService _userService;
    private readonly ITelegramBotClient _botClient;
    private readonly IChannelService _channelService;
    private readonly IMarkupHelperService _markupHelperService;
    private readonly ITextHelperService _textHelperService;
    private readonly IAttachmentService _attachmentService;
    private readonly TagRepository _tagRepository;
    private readonly IMediaGroupService _mediaGroupService;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="postService"></param>
    /// <param name="channelOptionService"></param>
    /// <param name="userService"></param>
    /// <param name="botClient"></param>
    /// <param name="channelService"></param>
    /// <param name="markupHelperService"></param>
    /// <param name="textHelperService"></param>
    /// <param name="attachmentService"></param>
    /// <param name="tagRepository"></param>
    /// <param name="mediaGroupService"></param>
    public PostController(
        ILogger<PostController> logger,
        IHttpContextAccessor httpContextAccessor,
        IPostService postService,
        IChannelOptionService channelOptionService,
        IUserService userService,
        ITelegramBotClient botClient,
        IChannelService channelService,
        IMarkupHelperService markupHelperService,
        ITextHelperService textHelperService,
        IAttachmentService attachmentService,
        TagRepository tagRepository,
        IMediaGroupService mediaGroupService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _postService = postService;
        _channelOptionService = channelOptionService;
        _userService = userService;
        _botClient = botClient;
        _channelService = channelService;
        _markupHelperService = markupHelperService;
        _textHelperService = textHelperService;
        _attachmentService = attachmentService;
        _tagRepository = tagRepository;
        _mediaGroupService = mediaGroupService;
    }

    /// <summary>
    /// 连接测试
    /// </summary>
    /// <returns></returns>
    [HttpPost("[action]")]
    public ActionResult<GenericResponse<TestTokenResponse>> TestToken()
    {
        var dbUser = _httpContextAccessor.GetUser();

        var response = new GenericResponse<TestTokenResponse> {
            Code = 0,
            Message = "成功",
            Result = new TestTokenResponse {
                UID = dbUser.Id,
                UserId = dbUser.UserID,
                UserName = dbUser.UserName,
                NickName = dbUser.FullName,
                UserRight = dbUser.Right,
                GroupId = dbUser.GroupID,
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// 创建稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    public async Task<ActionResult<GenericResponse<NewPosts>>> CreatePost([FromForm] CreatePostRequest post)
    {
        var dbUser = _httpContextAccessor.GetUser();
        if (!dbUser.Right.HasFlag(EUserRights.DirectPost))
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.Forbidden,
                Message = "当前用户没有直接发布权限, 无法通过 IPC 投稿",
            });
        }

        var userChatId = dbUser.PrivateChatID;
        if (userChatId == -1)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.InternalServerError,
                Message = "当前用户没有私聊过机器人, 无法接受投稿",
            });
        }

        var reviewGroup = _channelService.ReviewGroup;
        if (reviewGroup.Id == -1)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.InternalServerError,
                Message = "未设置审核群组, 无法接受投稿",
            });
        }

        var mediaCount = post.Media?.Count ?? 0;
        bool hasMedia = post.Media != null;
        switch (post.PostType)
        {
            case MessageType.Text:
            case MessageType.Photo:
            case MessageType.Audio:
            case MessageType.Video:
            case MessageType.Voice:
            case MessageType.Document:
                break;
            case MessageType.Animation:
                if (mediaCount > 1)
                {
                    mediaCount = 1;
                }
                break;
            default:
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "不支持的稿件类型"
                });
        }

        bool fromChannel;
        if (!string.IsNullOrEmpty(post.ChannelName) && !string.IsNullOrEmpty(post.ChannelTitle) && post.ChannelID != 0 && post.ChannelMsgID != 0)
        {
            // 如果消息来自频道就更新频道信息
            var option = await _channelOptionService.FetchChannelOption(post.ChannelID, post.ChannelTitle, post.ChannelName);
            if (option == EChannelOption.AutoReject)
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.Forbidden,
                    Message = "当前频道禁止投稿",
                });
            }

            fromChannel = option == EChannelOption.Normal;
        }
        else
        {
            fromChannel = false;
        }

        var postText = _textHelperService.PureText(post.Text);
        int newTags = _tagRepository.FetchTags(post.Text);

        var newPost = new NewPosts {
            OriginChatID = userChatId,
            OriginActionChatID = userChatId,
            ReviewChatID = userChatId,
            ReviewActionChatID = userChatId,
            Anonymous = dbUser.PreferAnonymous,
            Text = postText,
            RawText = post.Text ?? "",
            ChannelID = fromChannel ? post.ChannelID : -1,
            ChannelMsgID = fromChannel ? post.ChannelMsgID : -1,
            Status = EPostStatus.Reviewing,
            PostType = post.PostType,
            Tags = newTags,
            HasSpoiler = post.HasSpoiler,
            PosterUID = dbUser.UserID,
        };

        var newPostId = await _postService.Insertable(newPost).ExecuteReturnIdentityAsync();

        Message? originMsg = null;

        if (post.PostType == MessageType.Text) // 纯文本消息
        {
            if (string.IsNullOrEmpty(postText))
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "文本类型投稿的Text字段不允许为空",
                });
            }

            originMsg = await _botClient.SendTextMessageAsync(userChatId, postText, disableNotification: true, disableWebPagePreview: false);

        }
        else if (post.Media != null && mediaCount == 1) // 非媒体组消息
        {
            var fileStream = post.Media[0].OpenReadStream();
            var fileName = (post.MediaNames?.Any() == true && !string.IsNullOrEmpty(post.MediaNames[0])) ? post.MediaNames[0] : "Media";

            var inputFile = new InputFileStream(fileStream, fileName);
            var handler = post.PostType switch {
                MessageType.Photo => _botClient.SendPhotoAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                MessageType.Audio => _botClient.SendAudioAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, title: fileName),
                MessageType.Video => _botClient.SendVideoAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                MessageType.Voice => _botClient.SendVoiceAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html),
                MessageType.Document => _botClient.SendDocumentAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html),
                MessageType.Animation => _botClient.SendAnimationAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                _ => null,
            };

            if (handler == null)
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "不支持的稿件类型"
                });
            }

            originMsg = await handler;

            // 记录Attachment
            var attachment = _attachmentService.GenerateAttachment(originMsg, newPostId);
            if (attachment != null)
            {
                await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
            }
            else
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.InternalServerError,
                    Message = "生成附件失败"
                });
            }
        }
        else if (post.Media != null && mediaCount > 1) // 媒体组消息
        {
            post.MediaNames ??= new List<string>();

            for (int i = post.MediaNames.Count; i < mediaCount; i++)
            {
                post.MediaNames.Add($"Media {i}");
            }

            var group = new IAlbumInputMedia[mediaCount];
            for (int i = 0; i < mediaCount; i++)
            {
                var fileStream = post.Media[i].OpenReadStream();
                var fileName = !string.IsNullOrEmpty(post.MediaNames[i]) ? post.MediaNames[i] : "Media";

                var inputFile = new InputFileStream(fileStream, fileName);
                group[i] = post.PostType switch {
                    MessageType.Photo => new InputMediaPhoto(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = post.HasSpoiler },
                    MessageType.Audio => new InputMediaAudio(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                    MessageType.Video => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = post.HasSpoiler },
                    MessageType.Voice => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                    MessageType.Document => new InputMediaDocument(inputFile) { Caption = i == mediaCount - 1 ? postText : null, ParseMode = ParseMode.Html },
                    _ => throw new Exception("未知的稿件类型"),
                };
            }

            var messages = await _botClient.SendMediaGroupAsync(userChatId, group);
            originMsg = messages.First();

            // 记录Attachment
            var attachments = messages.Select(x => _attachmentService.GenerateAttachment(x, newPostId)).ToList();
            await _attachmentService.Storageable(attachments!).ExecuteCommandAsync();

            // 记录媒体组消息
            await _mediaGroupService.AddPostMediaGroup(messages);

            newPost.OriginMediaGroupID = originMsg.MediaGroupId ?? "";
            newPost.ReviewMediaGroupID = originMsg.MediaGroupId ?? "";
        }

        if (originMsg == null)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.BadRequest,
                Message = "非文本类型投稿的Media字段不允许为空",
            });
        }

        var keyboard = _markupHelperService.ReviewKeyboardA(newTags, post.HasSpoiler);
        string msg = _textHelperService.MakeReviewMessage(dbUser, newPost.Anonymous);

        var actionMsg = await _botClient.SendTextMessageAsync(userChatId, msg, parseMode: ParseMode.Html, disableWebPagePreview: true, replyToMessageId: originMsg.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

        newPost.Id = newPostId;
        newPost.OriginMsgID = originMsg.MessageId;
        newPost.ReviewMsgID = originMsg.MessageId;
        newPost.OriginActionMsgID = actionMsg.MessageId;
        newPost.ReviewActionMsgID = actionMsg.MessageId;
        newPost.ModifyAt = DateTime.Now;

        await _postService.Updateable(newPost).UpdateColumns(static x => new {
            x.ReviewMsgID,
            x.OriginActionMsgID,
            x.ReviewActionMsgID,
            x.OriginMediaGroupID,
            x.ReviewMediaGroupID,
            x.ModifyAt
        }).ExecuteCommandAsync();

        dbUser.PostCount++;
        dbUser.ModifyAt = DateTime.Now;
        await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();

        var response = new GenericResponse<NewPosts> {
            Code = HttpStatusCode.OK,
            Message = "成功",
            Success = true,
            Result = newPost,
        };

        return Ok(response);
    }

    /// <summary>
    /// 创建稿件
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    [HttpPut("[action]")]
    public async Task<ActionResult<GenericResponse<NewPosts>>> EditReviewMessage([FromForm] CreatePostRequest post)
    {
        var dbUser = _httpContextAccessor.GetUser();
        if (!dbUser.Right.HasFlag(EUserRights.DirectPost))
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.Forbidden,
                Message = "当前用户没有直接发布权限, 无法通过 IPC 投稿",
            });
        }

        var userChatId = dbUser.PrivateChatID;
        if (userChatId == -1)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.InternalServerError,
                Message = "当前用户没有私聊过机器人, 无法接受投稿",
            });
        }

        var reviewGroup = _channelService.ReviewGroup;
        if (reviewGroup.Id == -1)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.InternalServerError,
                Message = "未设置审核群组, 无法接受投稿",
            });
        }

        var mediaCount = post.Media?.Count ?? 0;
        bool hasMedia = post.Media != null;
        switch (post.PostType)
        {
            case MessageType.Text:
            case MessageType.Photo:
            case MessageType.Audio:
            case MessageType.Video:
            case MessageType.Voice:
            case MessageType.Document:
                break;
            case MessageType.Animation:
                if (mediaCount > 1)
                {
                    mediaCount = 1;
                }
                break;
            default:
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "不支持的稿件类型"
                });
        }

        bool fromChannel;
        if (!string.IsNullOrEmpty(post.ChannelName) && !string.IsNullOrEmpty(post.ChannelTitle) && post.ChannelID != 0 && post.ChannelMsgID != 0)
        {
            // 如果消息来自频道就更新频道信息
            var option = await _channelOptionService.FetchChannelOption(post.ChannelID, post.ChannelTitle, post.ChannelName);
            if (option == EChannelOption.AutoReject)
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.Forbidden,
                    Message = "当前频道禁止投稿",
                });
            }

            fromChannel = option == EChannelOption.Normal;
        }
        else
        {
            fromChannel = false;
        }

        var postText = _textHelperService.PureText(post.Text);
        int newTags = _tagRepository.FetchTags(post.Text);

        var newPost = new NewPosts {
            OriginChatID = userChatId,
            OriginActionChatID = userChatId,
            ReviewChatID = userChatId,
            ReviewActionChatID = userChatId,
            Anonymous = dbUser.PreferAnonymous,
            Text = postText,
            RawText = post.Text ?? "",
            ChannelID = fromChannel ? post.ChannelID : -1,
            ChannelMsgID = fromChannel ? post.ChannelMsgID : -1,
            Status = EPostStatus.Reviewing,
            PostType = post.PostType,
            Tags = newTags,
            HasSpoiler = post.HasSpoiler,
            PosterUID = dbUser.UserID,
        };

        var newPostId = await _postService.Insertable(newPost).ExecuteReturnIdentityAsync();

        Message? originMsg = null;

        if (post.PostType == MessageType.Text) // 纯文本消息
        {
            if (string.IsNullOrEmpty(postText))
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "文本类型投稿的Text字段不允许为空",
                });
            }

            originMsg = await _botClient.SendTextMessageAsync(userChatId, postText, disableNotification: true, disableWebPagePreview: false);

        }
        else if (post.Media != null && mediaCount == 1) // 非媒体组消息
        {
            var fileStream = post.Media[0].OpenReadStream();
            var fileName = (post.MediaNames?.Any() == true && !string.IsNullOrEmpty(post.MediaNames[0])) ? post.MediaNames[0] : "Media";

            var inputFile = new InputFileStream(fileStream, fileName);
            var handler = post.PostType switch {
                MessageType.Photo => _botClient.SendPhotoAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                MessageType.Audio => _botClient.SendAudioAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, title: fileName),
                MessageType.Video => _botClient.SendVideoAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                MessageType.Voice => _botClient.SendVoiceAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html),
                MessageType.Document => _botClient.SendDocumentAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html),
                MessageType.Animation => _botClient.SendAnimationAsync(userChatId, inputFile, caption: postText, parseMode: ParseMode.Html, hasSpoiler: post.HasSpoiler),
                _ => null,
            };

            if (handler == null)
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.BadRequest,
                    Message = "不支持的稿件类型"
                });
            }

            originMsg = await handler;

            // 记录Attachment
            var attachment = _attachmentService.GenerateAttachment(originMsg, newPostId);
            if (attachment != null)
            {
                await _attachmentService.Insertable(attachment).ExecuteCommandAsync();
            }
            else
            {
                return BadRequest(new GenericResponse {
                    Code = HttpStatusCode.InternalServerError,
                    Message = "生成附件失败"
                });
            }
        }
        else if (post.Media != null && mediaCount > 1) // 媒体组消息
        {
            post.MediaNames ??= new List<string>();

            for (int i = post.MediaNames.Count; i < mediaCount; i++)
            {
                post.MediaNames.Add($"Media {i}");
            }

            var group = new IAlbumInputMedia[mediaCount];
            for (int i = 0; i < mediaCount; i++)
            {
                var fileStream = post.Media[i].OpenReadStream();
                var fileName = !string.IsNullOrEmpty(post.MediaNames[i]) ? post.MediaNames[i] : "Media";

                var inputFile = new InputFileStream(fileStream, fileName);
                group[i] = post.PostType switch {
                    MessageType.Photo => new InputMediaPhoto(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = post.HasSpoiler },
                    MessageType.Audio => new InputMediaAudio(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                    MessageType.Video => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html, HasSpoiler = post.HasSpoiler },
                    MessageType.Voice => new InputMediaVideo(inputFile) { Caption = i == 0 ? postText : null, ParseMode = ParseMode.Html },
                    MessageType.Document => new InputMediaDocument(inputFile) { Caption = i == mediaCount - 1 ? postText : null, ParseMode = ParseMode.Html },
                    _ => throw new Exception("未知的稿件类型"),
                };
            }

            var messages = await _botClient.SendMediaGroupAsync(userChatId, group);
            originMsg = messages.First();

            // 记录Attachment
            var attachments = messages.Select(x => _attachmentService.GenerateAttachment(x, newPostId)).ToList();
            await _attachmentService.Storageable(attachments!).ExecuteCommandAsync();

            // 记录媒体组消息
            await _mediaGroupService.AddPostMediaGroup(messages);

            newPost.OriginMediaGroupID = originMsg.MediaGroupId ?? "";
            newPost.ReviewMediaGroupID = originMsg.MediaGroupId ?? "";
        }

        if (originMsg == null)
        {
            return BadRequest(new GenericResponse {
                Code = HttpStatusCode.BadRequest,
                Message = "非文本类型投稿的Media字段不允许为空",
            });
        }

        var keyboard = _markupHelperService.ReviewKeyboardA(newTags, post.HasSpoiler);
        string msg = _textHelperService.MakeReviewMessage(dbUser, newPost.Anonymous);

        var actionMsg = await _botClient.SendTextMessageAsync(userChatId, msg, parseMode: ParseMode.Html, disableWebPagePreview: true, replyToMessageId: originMsg.MessageId, replyMarkup: keyboard, allowSendingWithoutReply: true);

        newPost.Id = newPostId;
        newPost.OriginMsgID = originMsg.MessageId;
        newPost.ReviewMsgID = originMsg.MessageId;
        newPost.OriginActionMsgID = actionMsg.MessageId;
        newPost.ReviewActionMsgID = actionMsg.MessageId;
        newPost.ModifyAt = DateTime.Now;

        await _postService.Updateable(newPost).UpdateColumns(static x => new {
            x.ReviewMsgID,
            x.OriginActionMsgID,
            x.ReviewActionMsgID,
            x.OriginMediaGroupID,
            x.ReviewMediaGroupID,
            x.ModifyAt
        }).ExecuteCommandAsync();

        dbUser.PostCount++;
        dbUser.ModifyAt = DateTime.Now;
        await _userService.Updateable(dbUser).UpdateColumns(static x => new { x.PostCount, x.ModifyAt }).ExecuteCommandAsync();

        var response = new GenericResponse<NewPosts> {
            Code = HttpStatusCode.OK,
            Message = "成功",
            Success = true,
            Result = newPost,
        };

        return Ok(response);
    }

}
