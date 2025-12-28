using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command;

/// <summary>
/// 投稿命令
/// </summary>
[AppService(LifeTime.Scoped)]
public class PostCommand(
    ITelegramBotService _botClient,
    IUserService _userService,
    IChannelService _channelService,
    IPostService _postService,
    IMarkupHelperService _markupHelperService,
    IAttachmentService _attachmentService,
    ITextHelperService _textHelperService,
    IMediaGroupService _mediaGroupService)
{

    /// <summary>
    /// 投稿消息处理
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    [QueryCmd("POST", EUserRights.SendPost, Description = "投稿消息处理")]
    public async Task HandlePostQuery(Users dbUser, CallbackQuery query)
    {
        var message = query.Message!;
        var post = await _postService.FetchPostFromCallbackQuery(query).ConfigureAwait(false);

        if (post == null)
        {
            await _botClient.AutoReply("未找到稿件", query).ConfigureAwait(false);
            await _botClient.EditMessageReplyMarkup(message.Chat, message.Id, null).ConfigureAwait(false);
            return;
        }

        if (post.Status == EPostStatus.ReviewTimeout || post.Status == EPostStatus.ConfirmTimeout)
        {
            var msg = "该稿件已过期, 无法操作";
            await _botClient.AutoReply(msg, query).ConfigureAwait(false);
            await _botClient.EditMessageText(message, msg).ConfigureAwait(false);
            return;
        }

        if (post.Status != EPostStatus.Padding)
        {
            await _botClient.AutoReply("请不要重复操作", query, true).ConfigureAwait(false);
            await _botClient.EditMessageReplyMarkup(message.Chat, message.Id, null).ConfigureAwait(false);
            return;
        }

        if (post.PosterUID != dbUser.UserID)
        {
            await _botClient.AutoReply("这不是你的稿件", query).ConfigureAwait(false);
            return;
        }

        switch (query.Data)
        {
            case "post anymouse":
                await SetAnymouse(post, query).ConfigureAwait(false);
                break;
            case "post cancel":
                await CancelPost(post, query).ConfigureAwait(false);
                break;
            case "post confirm":
                await ConfirmPost(post, dbUser, query).ConfigureAwait(false);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 设置或者取消匿名
    /// </summary>
    /// <param name="post"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    private async Task SetAnymouse(NewPosts post, CallbackQuery query)
    {
        await _botClient.AutoReply("可以使用命令 /anonymous 切换默认匿名投稿", query).ConfigureAwait(false);

        bool anonymous = !post.Anonymous;
        await _postService.SetPostAnonymous(post, anonymous).ConfigureAwait(false);

        var keyboard = _markupHelperService.PostKeyboard(anonymous);
        await _botClient.EditMessageReplyMarkup(query.Message!.Chat, query.Message!.Id, keyboard).ConfigureAwait(false);
    }

    /// <summary>
    /// 取消投稿
    /// </summary>
    /// <param name="post"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    private async Task CancelPost(NewPosts post, CallbackQuery query)
    {
        await _postService.CancelPost(post).ConfigureAwait(false);

        await _botClient.EditMessageText(query.Message!, Langs.PostCanceled, replyMarkup: null).ConfigureAwait(false);

        await _botClient.AutoReply(Langs.PostCanceled, query).ConfigureAwait(false);
    }

    /// <summary>
    /// 确认投稿
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="post"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    private async Task ConfirmPost(NewPosts post, Users dbUser, CallbackQuery query)
    {
        if (await _postService.CheckPostLimit(dbUser, null, query).ConfigureAwait(false) == false)
        {
            return;
        }

        Message reviewMsg;
        if (!post.IsMediaGroup)
        {
            reviewMsg = await _botClient.ForwardMessage(_channelService.ReviewGroup.Id, post.OriginChatID, (int)post.OriginMsgID).ConfigureAwait(false);
        }
        else
        {
            var attachments = await _attachmentService.FetchAttachmentsByPostId(post.Id).ConfigureAwait(false);
            var group = new IAlbumInputMedia[attachments.Count];
            for (int i = 0; i < attachments.Count; i++)
            {
                var attachmentType = attachments[i].Type;
                if (attachmentType == MessageType.Unknown)
                {
                    attachmentType = post.PostType;
                }

                group[i] = attachmentType switch {
                    MessageType.Photo => new InputMediaPhoto(new InputFileId(attachments[i].FileID)) {
                        Caption = i == 0 ? post.Text : null,
                        ParseMode = ParseMode.Html
                    },
                    MessageType.Audio => new InputMediaAudio(new InputFileId(attachments[i].FileID)) {
                        Caption = i == 0 ? post.Text : null,
                        ParseMode = ParseMode.Html
                    },
                    MessageType.Video => new InputMediaVideo(new InputFileId(attachments[i].FileID)) {
                        Caption = i == 0 ? post.Text : null,
                        ParseMode = ParseMode.Html
                    },
                    MessageType.Document => new InputMediaDocument(new InputFileId(attachments[i].FileID)) {
                        Caption = i == attachments.Count - 1 ? post.Text : null,
                        ParseMode = ParseMode.Html
                    },
                    _ => throw new Exception("未知的稿件类型"),
                };
            }
            var messages = await _botClient.SendMediaGroup(_channelService.ReviewGroup, group).ConfigureAwait(false);
            reviewMsg = messages.First();
            post.ReviewMediaGroupID = reviewMsg.MediaGroupId ?? "";

            //记录媒体组消息
            await _mediaGroupService.AddPostMediaGroup(messages).ConfigureAwait(false);
        }

        string msg = _textHelperService.MakeReviewMessage(dbUser, post.Anonymous);

        bool? hasSpoiler = post.CanSpoiler ? post.HasSpoiler : null;
        var keyboard = _markupHelperService.ReviewKeyboardA(post.Tags, hasSpoiler);



        var manageMsg = await _botClient.SendMessage(reviewMsg, msg, parseMode: ParseMode.Html, true, disableWebPagePreview: true, replyMarkup: keyboard).ConfigureAwait(false);

        post.ReviewChatID = reviewMsg.Chat.Id;
        post.ReviewMsgID = reviewMsg.MessageId;
        post.ReviewActionChatID = manageMsg.Chat.Id;
        post.ReviewActionMsgID = manageMsg.MessageId;
        post.Status = EPostStatus.Reviewing;
        post.ModifyAt = DateTime.Now;
        await _postService.Updateable(post).UpdateColumns(static x => new {
            x.ReviewChatID,
            x.ReviewMsgID,
            x.ReviewActionChatID,
            x.ReviewActionMsgID,
            x.ReviewMediaGroupID,
            x.Status,
            x.ModifyAt
        }).ExecuteCommandAsync().ConfigureAwait(false);

        await _botClient.AutoReply(Langs.PostSendSuccess, query).ConfigureAwait(false);
        await _botClient.EditMessageText(query.Message!, Langs.ThanksForSendingPost, replyMarkup: null).ConfigureAwait(false);

        dbUser.PostCount++;
        await _userService.UpdateUserPostCount(dbUser).ConfigureAwait(false);
    }
}
