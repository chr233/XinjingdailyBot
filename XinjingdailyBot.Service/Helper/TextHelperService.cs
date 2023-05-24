using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper
{
    [AppService(typeof(ITextHelperService), LifeTime.Transient)]
    public sealed class TextHelperService : ITextHelperService
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

        private static readonly Regex MatchTag = new(@"(^#\S+)|(\s#\S+)");
        private static readonly Regex MatchSpace = new(@"^\s*$");

        public string PureText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (PureHashTag)
            {
                //过滤HashTag
                text = MatchTag.Replace(text, "");
            }

            if (PureReturns)
            {
                //过滤连续换行
                var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Where(x => !MatchSpace.IsMatch(x)).Select(x => x.Trim());
                text = string.Join('\n', parts);
            }

            return text;
        }

        public string HtmlUserLink(long userId, string userName, string userNick)
        {
            var nick = EscapeHtml(userNick);

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

        public string RejectReasonToString(ERejectReason rejectReason)
        {
            var reason = rejectReason switch
            {
                ERejectReason.Fuzzy => "图片模糊/看不清",
                ERejectReason.Duplicate => "重复的稿件",
                ERejectReason.Boring => "内容不够有趣",
                ERejectReason.Confused => "审核没看懂,建议配文说明",
                ERejectReason.Deny => "不合适发布的内容",
                ERejectReason.QRCode => "稿件包含二维码水印",
                ERejectReason.Other => "其他原因",
                ERejectReason.CustomReason => "其他原因",
                ERejectReason.AutoReject => "稿件审核超时, 自动拒绝",
                _ => "未知",
            };
            return reason;
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

        public string MakePoster(OldPosts post, Users poster)
        {
            var user = HtmlUserLink(poster);

            if (post.IsFromChannel)
            {
                var channel = HtmlUserLink(0, post.ChannelName, post.ChannelTitle);
                if (post.Anonymous)
                {
                    return $"<i>from</i> {channel}";
                }
                else
                {
                    return $"<i>from</i> {channel} <i>via</i> {user}";
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

        public string MakePostText(OldPosts post, Users poster)
        {
            var tag = _tagRepository.GetActiviedHashTags(post.NewTags);

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

            var from = MakePoster(post, poster);

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

            if (entities == null)
            {
                return text;
            }
            else
            {
                return ParseMessage(entities, text);
            }
        }

        /// <summary>
        /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ParseMessage(MessageEntity[] entities, string text)
        {
            StringBuilder sb = new(text.Replace('<', '＜').Replace('>', '＞').Replace('&', '＆'));

            Dictionary<int, TagObjct> tagMap = new();

            int count = entities.Length;


            for (int i = 0; i < count; i++)
            {
                var entity = entities[i];
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
                        head = $"<a href=\"{EscapeHtml(entity.Url)}\">";
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
