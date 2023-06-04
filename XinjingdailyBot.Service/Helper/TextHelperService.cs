using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper
{
    /// <inheritdoc cref="ITextHelperService"/>
    [AppService(typeof(ITextHelperService), LifeTime.Transient)]
    internal sealed class TextHelperService : ITextHelperService
    {
        private readonly IChannelService _channelService;
        private readonly TagRepository _tagRepository;

        public TextHelperService(
            IChannelService channelService,
            IOptions<OptionsSetting> options,
            TagRepository tagRepository)
        {
            _channelService = channelService;
            _tagRepository = tagRepository;

            var postOption = options.Value.Post;
            PureWords = postOption.PureWords.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            PureReturns = postOption.PureReturns;
            PureHashTag = postOption.PureHashTag;
        }

        private string[] PureWords { get; init; }
        private bool PureReturns { get; init; }
        private bool PureHashTag { get; init; }

        public string PureText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (PureHashTag)
            {
                //过滤HashTag
                text = RegexUtils.MatchHashTag().Replace(text, "");
            }

            if (PureReturns)
            {
                var matchSpace = RegexUtils.MatchBlankLine();
                //过滤连续换行
                var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Where(x => !matchSpace.IsMatch(x)).Select(x => x.Trim());
                text = string.Join('\n', parts);
            }

            return text;
        }

        public string HtmlUserLink(long userId, string userName, string userNick)
        {
            var nick = EscapeHtml(userNick).ReEscapeHtml();

            if (string.IsNullOrEmpty(userName))
            {
                return HtmlLink($"tg://user?id={userId}", nick);
            }
            else
            {
                return HtmlLink($"https://t.me/{userName}", nick);
            }
        }

        public string HtmlLink(string url, string text)
        {
            return $"<a href=\"{url}\">{text}</a>";
        }

        public string HtmlUserLink(Users user)
        {
            return HtmlUserLink(user.UserID, user.UserName, user.FullName);
        }

        public string HtmlMessageLink(long messageID, string chatName, string linkName)
        {
            return $"<a href=\"https://t.me/{chatName}/{messageID}\">{linkName}</a>";
        }

        public string EscapeHtml(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            else
            {
                var escapedText = text
                    .Replace("<", "＜")
                    .Replace(">", "＞")
                    .Replace("&", "＆");
                foreach (var item in PureWords)
                {
                    escapedText = escapedText.Replace(item, "");
                }
                return escapedText;
            }
        }

        public string MakeReviewMessage(Users poster, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "待审核";

            var msg = string.Join('\n', $"#待审核 ", $"投稿人: {pUser}", "", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var rUser = HtmlUserLink(reviewer);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "已发布";

            var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        public string MakeReviewMessage(Users poster, long messageID, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var msgLink = HtmlMessageLink(messageID, _channelService.AcceptChannel.Username ?? _channelService.AcceptChannel.Id.ToString(), "消息直链");
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "已发布";

            var msg = string.Join('\n', $"发布人: {pUser}", $"消息: {msgLink}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason)
        {
            var pUser = HtmlUserLink(poster);
            var rUser = HtmlUserLink(reviewer);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = $"已拒绝 {rejectReason}";

            var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        public string MakeNotification(bool isDirect, long messageID)
        {
            var msgLink = HtmlMessageLink(messageID, _channelService.AcceptChannel.Username ?? _channelService.AcceptChannel.Id.ToString(), "消息直链");

            return isDirect ? $"稿件已发布, {msgLink}" : $"稿件已通过, 感谢您的支持 {msgLink}";
        }

        public string MakeNotification(string reason)
        {
            var msg = string.Join('\n', "稿件未通过", $"原因: {reason}");
            return msg;
        }

        public string MakePoster(NewPosts post, Users poster, ChannelOptions? channel)
        {
            var user = HtmlUserLink(poster);

            if (post.IsFromChannel && !string.IsNullOrEmpty(channel?.ChannelName))
            {
                var link = HtmlMessageLink(post.ChannelMsgId, channel.ChannelName, channel.ChannelTitle);
                if (post.Anonymous)
                {
                    return $"<i>from</i> {link}";
                }
                else
                {
                    return $"<i>from</i> {link} <i>via</i> {user}";
                }
            }
            else
            {
                if (post.Anonymous)
                {
                    return $"<i>via</i> 匿名";
                }
                else
                {
                    return $"<i>via</i> {user}";
                }
            }
        }

        public string MakePostText(NewPosts post, Users poster, ChannelOptions? channel)
        {
            var tag = _tagRepository.GetActiviedHashTags(post.Tags);

            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(tag))
            {
                sb.AppendLine(tag);
            }

            if (!string.IsNullOrEmpty(post.Text))
            {
                var text = post.Text;
                sb.AppendLine(text);
            }

            var from = MakePoster(post, poster, channel);

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.AppendLine(from);

            return sb.ToString();
        }

        public string ParseMessage(Message message)
        {
            MessageEntity[]? entities;
            string? text;

            if (message.Type == MessageType.Text)
            {
                text = message.Text;
                entities = message.Entities;
            }
            else
            {
                text = message.Caption;
                entities = message.CaptionEntities;
            }

            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            return ParseMessage(entities, text).ReEscapeHtml();
        }

        /// <summary>
        /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ParseMessage(MessageEntity[]? entities, string text)
        {
            var sb = new StringBuilder(text.EscapeHtml());

            if (entities == null)
            {
                return sb.ToString();
            }

            var tagMap = new Dictionary<int, TagObjct>();
            foreach (var entity in entities)
            {
                string head;
                string tail;

                switch (entity.Type)
                {
                    case MessageEntityType.Bold:
                        head = "<b>";
                        tail = "</b>";
                        break;
                    case MessageEntityType.Italic:
                        head = "<i>";
                        tail = "</i>";
                        break;
                    case MessageEntityType.Underline:
                        head = "<u>";
                        tail = "</u>";
                        break;
                    case MessageEntityType.Strikethrough:
                        head = "<s>";
                        tail = "</s>";
                        break;
                    case MessageEntityType.Spoiler:
                        head = "<tg-spoiler>";
                        tail = "</tg-spoiler>";
                        break;
                    case MessageEntityType.TextLink:
                        head = $"<a href=\"{EscapeHtml(entity.Url).ReEscapeHtml()}\">";
                        tail = "</a>";
                        break;
                    case MessageEntityType.Code:
                        head = "<code>";
                        tail = "</code>";
                        break;
                    case MessageEntityType.Pre:
                        head = "<pre>";
                        tail = "</pre>";
                        break;

                    default:
                        continue;
                }

                int start = entity.Offset;
                int end = entity.Offset + entity.Length;

                if (!tagMap.ContainsKey(start))
                {
                    tagMap.Add(start, new(head));
                }
                else
                {
                    tagMap[start].AddLast(head);
                }

                if (!tagMap.ContainsKey(end))
                {
                    tagMap.Add(end, new(tail));
                }
                else
                {
                    tagMap[end].AddFirst(tail);
                }
            }

            var indexList = tagMap.Keys.ToArray().OrderByDescending(x => x);

            foreach (var index in indexList)
            {
                sb.Insert(index, tagMap[index].ToString());
            }

            return PureText(sb.ToString());
        }
    }
}
