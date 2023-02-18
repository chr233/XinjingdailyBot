using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Localization;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Command
{
    [AppService(LifeTime.Scoped)]
    public class ReviewCommand
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IUserService _userService;
        private readonly IChannelService _channelService;
        private readonly IPostService _postService;
        private readonly ITextHelperService _textHelperService;
        private readonly IMarkupHelperService _markupHelperService;

        public ReviewCommand(
            ITelegramBotClient botClient,
            IUserService userService,
            IChannelService channelService,
            IPostService postService,
            ITextHelperService textHelperService,
            IMarkupHelperService markupHelperService)
        {
            _botClient = botClient;
            _userService = userService;
            _channelService = channelService;
            _postService = postService;
            _textHelperService = textHelperService;
            _markupHelperService = markupHelperService;
        }

        /// <summary>
        /// 自定义拒绝稿件理由
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("NO", UserRights.ReviewPost, Description = "自定义拒绝稿件理由")]
        public async Task ResponseNo(Users dbUser, Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入拒绝理由";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);

                if (post == null)
                {
                    return "未找到稿件";
                }

                if (post.Status != PostStatus.Reviewing)
                {
                    return "仅能编辑状态为审核中的稿件";
                }

                var reason = string.Join(' ', args).Trim();

                if (string.IsNullOrEmpty(reason))
                {
                    return "请输入拒绝理由";
                }

                post.Reason = RejectReason.CustomReason;
                await _postService.RejetPost(post, dbUser, reason);

                return $"已拒绝该稿件, 理由: {reason}";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }

        /// <summary>
        /// 修改稿件文字说明
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [TextCmd("EDIT", UserRights.ReviewPost, Description = "修改稿件文字说明")]
        public async Task ResponseEditPost(Message message, string[] args)
        {
            async Task<string> exec()
            {
                if (message.Chat.Type != ChatType.Private && message.Chat.Id != _channelService.ReviewGroup.Id)
                {
                    return "该命令仅限审核群内使用";
                }

                if (message.ReplyToMessage == null)
                {
                    return "请回复审核消息并输入需要替换的描述";
                }

                var messageId = message.ReplyToMessage.MessageId;

                var post = await _postService.Queryable().FirstAsync(x => x.ReviewMsgID == messageId || x.ManageMsgID == messageId);
                if (post == null)
                {
                    return "未找到稿件";
                }

                if (post.Status != PostStatus.Reviewing)
                {
                    return "仅能编辑状态为审核中的稿件";
                }

                var postUser = await _userService.FetchUserByUserID(post.PosterUID);
                if (postUser == null)
                {
                    return "未找到投稿用户";
                }

                post.Text = string.Join(' ', args).Trim();
                await _postService.Updateable(post).UpdateColumns(x => new { x.Text }).ExecuteCommandAsync();

                return $"稿件描述已更新";
            }

            var text = await exec();
            await _botClient.SendCommandReply(text, message, false);
        }

        /// <summary>
        /// 处理稿件
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        [QueryCmd("REVIEW", UserRights.ReviewPost, Alias = "REJECT", Description = "审核稿件")]
        public async Task HandleQuery(Users dbUser, CallbackQuery callbackQuery)
        {
            Message message = callbackQuery.Message!;
            Posts? post = await _postService.Queryable().FirstAsync(x => x.ManageMsgID == message.MessageId);

            if (post == null)
            {
                await _botClient.AutoReplyAsync("未找到稿件", callbackQuery);
                await _botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (post.Status != PostStatus.Reviewing)
            {
                await _botClient.AutoReplyAsync("请不要重复操作", callbackQuery);
                await _botClient.EditMessageReplyMarkupAsync(message, null);
                return;
            }

            if (!dbUser.Right.HasFlag(UserRights.ReviewPost))
            {
                await _botClient.AutoReplyAsync("无权操作", callbackQuery);
                return;
            }

            switch (callbackQuery.Data)
            {
                case "review reject":
                    await SwitchKeyboard(true, post, callbackQuery);
                    break;
                case "reject back":
                    await SwitchKeyboard(false, post, callbackQuery);
                    break;

                case "review tag nsfw":
                    await SetPostTag(post, BuildInTags.NSFW, callbackQuery);
                    break;
                case "review tag wanan":
                    await SetPostTag(post, BuildInTags.WanAn, callbackQuery);
                    break;
                case "review tag friend":
                    await SetPostTag(post, BuildInTags.Friend, callbackQuery);
                    break;
                case "review tag ai":
                    await SetPostTag(post, BuildInTags.AIGraph, callbackQuery);
                    break;
                case "review tag spoiler":
                    await SetPostTag(post, BuildInTags.Spoiler, callbackQuery);
                    break;

                case "reject fuzzy":
                    await RejectPostHelper(post, dbUser, RejectReason.Fuzzy);
                    break;
                case "reject duplicate":
                    await RejectPostHelper(post, dbUser, RejectReason.Duplicate);
                    break;
                case "reject boring":
                    await RejectPostHelper(post, dbUser, RejectReason.Boring);
                    break;
                case "reject confusing":
                    await RejectPostHelper(post, dbUser, RejectReason.Confused);
                    break;
                case "reject deny":
                    await RejectPostHelper(post, dbUser, RejectReason.Deny);
                    break;
                case "reject qrcode":
                    await RejectPostHelper(post, dbUser, RejectReason.QRCode);
                    break;
                case "reject other":
                    await RejectPostHelper(post, dbUser, RejectReason.Other);
                    break;

                case "review accept":
                    await _postService.AcceptPost(post, dbUser, callbackQuery);
                    break;

                case "review anymouse":
                    await SetAnymouse(post, callbackQuery);
                    break;

                case "review cancel":
                    await CancelPost(post, callbackQuery);
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
        private async Task SetAnymouse(Posts post, CallbackQuery query)
        {
            await _botClient.AutoReplyAsync("可以使用命令 /anymouse 切换默认匿名投稿", query);

            bool anonymous = !post.Anonymous;
            post.Anonymous = anonymous;
            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Anonymous, x.ModifyAt }).ExecuteCommandAsync();

            bool hasSpoiler = post.PostType == MessageType.Photo || post.PostType == MessageType.Video;

            var keyboard = hasSpoiler ? _markupHelperService.DirectPostKeyboardWithSpoiler(anonymous, post.Tags) : _markupHelperService.DirectPostKeyboard(anonymous, post.Tags);
            await _botClient.EditMessageReplyMarkupAsync(query.Message!, keyboard);
        }

        /// <summary>
        /// 取消投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task CancelPost(Posts post, CallbackQuery query)
        {
            post.Status = PostStatus.Cancel;
            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Status, x.ModifyAt }).ExecuteCommandAsync();

            await _botClient.EditMessageTextAsync(query.Message!, Langs.PostCanceled, replyMarkup: null);

            await _botClient.AutoReplyAsync(Langs.PostCanceled, query);
        }

        /// <summary>
        /// 拒绝稿件包装方法
        /// </summary>
        /// <param name="post"></param>
        /// <param name="dbUser"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        private async Task RejectPostHelper(Posts post, Users dbUser, RejectReason rejectReason)
        {
            post.Reason = rejectReason;
            string reason = _textHelperService.RejectReasonToString(rejectReason);
            await _postService.RejetPost(post, dbUser, reason);
        }

        /// <summary>
        /// 设置inlineKeyboard
        /// </summary>
        /// <param name="rejectMode"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task SwitchKeyboard(bool rejectMode, Posts post, CallbackQuery callbackQuery)
        {
            if (rejectMode)
            {
                await _botClient.AutoReplyAsync("请选择拒稿原因", callbackQuery);
            }

            bool hasSpoiler = post.Tags.HasFlag(BuildInTags.Spoiler);

            var keyboard = rejectMode ?
                _markupHelperService.ReviewKeyboardB() :
                (hasSpoiler ? _markupHelperService.ReviewKeyboardAWithSpoiler(post.Tags) : _markupHelperService.ReviewKeyboardA(post.Tags));

            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }

        /// <summary>
        /// 修改Tag
        /// </summary>
        /// <param name="post"></param>
        /// <param name="tag"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private async Task SetPostTag(Posts post, BuildInTags tag, CallbackQuery callbackQuery)
        {
            if (post.Tags.HasFlag(tag))
            {
                post.Tags &= ~tag;
            }
            else
            {
                post.Tags |= tag;
            }

            List<string> tagNames = new() { "当前标签:" };
            if (post.Tags.HasFlag(BuildInTags.NSFW))
            {
                tagNames.Add("NSFW");
            }
            if (post.Tags.HasFlag(BuildInTags.WanAn))
            {
                tagNames.Add("晚安");
            }
            if (post.Tags.HasFlag(BuildInTags.Friend))
            {
                tagNames.Add("我有一个朋友");
            }
            if (post.Tags.HasFlag(BuildInTags.AIGraph))
            {
                tagNames.Add("AI怪图");
            }
            if (post.Tags == BuildInTags.None)
            {
                tagNames.Add("无");
            }

            if (post.Tags.HasFlag(BuildInTags.Spoiler))
            {
                tagNames.Insert(0, "[启用遮罩]");
            }

            post.ModifyAt = DateTime.Now;
            await _postService.Updateable(post).UpdateColumns(x => new { x.Tags, x.ModifyAt }).ExecuteCommandAsync();

            await _botClient.AutoReplyAsync(string.Join(' ', tagNames), callbackQuery);

            bool hasSpoiler = post.PostType == MessageType.Photo || post.PostType == MessageType.Video;
            var keyboard =
                post.IsDirectPost ?
                (hasSpoiler ? _markupHelperService.DirectPostKeyboardWithSpoiler(post.Anonymous, post.Tags) : _markupHelperService.DirectPostKeyboard(post.Anonymous, post.Tags)) :
                (hasSpoiler ? _markupHelperService.ReviewKeyboardAWithSpoiler(post.Tags) : _markupHelperService.ReviewKeyboardA(post.Tags));
            await _botClient.EditMessageReplyMarkupAsync(callbackQuery.Message!, keyboard);
        }
    }
}
