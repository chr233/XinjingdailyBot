using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Localization;
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

        private static readonly string NSFWWrning = $"{Emojis.Warning} NSFW 提前预警 {Emojis.Warning}";

        string ITextHelperService.NSFWWrning { get => NSFWWrning; }

        /// <summary>
        /// 去除所有HashTag和连续换行
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 提取Tag
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public BuildInTags FetchTags(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BuildInTags.None;
            }

            var tags = BuildInTags.None;

            if (text.Contains("NSFW", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.NSFW;
            }
            if (text.Contains("朋友", StringComparison.InvariantCultureIgnoreCase) || text.Contains("英雄", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.Friend;
            }
            if (text.Contains("晚安", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.WanAn | BuildInTags.NSFW;
            }
            if (text.Contains("#ai", StringComparison.InvariantCultureIgnoreCase))
            {
                tags |= BuildInTags.AIGraph;
            }

            return tags;
        }

        /// <summary>
        /// Html格式的用户链接
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userNick"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Html链接
        /// </summary>
        /// <param name="url"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public string HtmlLink(string url, string text)
        {
            return $"<a href=\"{url}\">{text}</a>";
        }

        /// <summary>
        /// Html格式的用户链接
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string HtmlUserLink(Users user)
        {
            return HtmlUserLink(user.UserID, user.UserName, user.FullName);
        }

        /// <summary>
        /// HTML格式的消息链接
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="chatName"></param>
        /// <param name="linkName"></param>
        /// <returns></returns>
        public string HtmlMessageLink(long messageID, string chatName, string linkName)
        {
            return $"<a href=\"https://t.me/{chatName}/{messageID}\">{linkName}</a>";
        }

        /// <summary>
        /// HTML转义
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 生成审核消息(待审核)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        public string MakeReviewMessage(Users poster, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "待审核";

            var msg = string.Join('\n', $"投稿人: {pUser}", "", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核通过)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var rUser = HtmlUserLink(reviewer);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "已发布";

            var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核通过, 直接发布)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        public string MakeReviewMessage(Users poster, long messageID, bool anymouse)
        {
            var pUser = HtmlUserLink(poster);
            var msgLink = HtmlMessageLink(messageID, _channelService.AcceptChannel.Username ?? _channelService.AcceptChannel.Id.ToString(), "消息直链");
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = "已发布";

            var msg = string.Join('\n', $"发布人: {pUser}", $"消息: {msgLink}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核未通过)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <returns></returns>
        public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason)
        {
            var pUser = HtmlUserLink(poster);
            var rUser = HtmlUserLink(reviewer);
            var strAny = anymouse ? "匿名投稿" : "保留来源";
            var status = $"已拒绝 {rejectReason}";

            var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 格式化RejectReason
        /// </summary>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        public string RejectReasonToString(RejectReason rejectReason)
        {
            var reason = rejectReason switch
            {
                RejectReason.Fuzzy => "图片模糊/看不清",
                RejectReason.Duplicate => "重复的稿件",
                RejectReason.Boring => "内容不够有趣",
                RejectReason.Confused => "审核没看懂,建议配文说明",
                RejectReason.Deny => "不合适发布的内容",
                RejectReason.QRCode => "稿件包含二维码水印",
                RejectReason.Other => "其他原因",
                RejectReason.CustomReason => "其他原因",
                RejectReason.AutoReject => "稿件审核超时, 自动拒绝",
                _ => "未知",
            };
            return reason;
        }

        /// <summary>
        /// 生成通知消息(审核通过）
        /// </summary>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        public string MakeNotification(bool isDirect, long messageID)
        {
            var msgLink = HtmlMessageLink(messageID, _channelService.AcceptChannel.Username ?? _channelService.AcceptChannel.Id.ToString(), "消息直链");

            return isDirect ? $"稿件已发布, {msgLink}" : $"稿件已通过, 感谢您的支持 {msgLink}";
        }

        /// <summary>
        /// 生成通知消息(审核未通过）
        /// </summary>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        public string MakeNotification(string reason)
        {
            var msg = string.Join('\n', "稿件未通过", $"原因: {reason}");
            return msg;
        }

        /// <summary>
        /// 格式化BuildInTags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public string TagsToString(BuildInTags tags)
        {
            if (tags == BuildInTags.None)
            {
                return "";
            }

            List<string> tag = new();
            if (tags.HasFlag(BuildInTags.NSFW))
            {
                tag.Add("#NSFW");
            }
            if (tags.HasFlag(BuildInTags.Friend))
            {
                tag.Add("#我有一个朋友");
            }
            if (tags.HasFlag(BuildInTags.WanAn))
            {
                tag.Add("#晚安");
            }
            if (tags.HasFlag(BuildInTags.AIGraph))
            {
                tag.Add("#AI怪图");
            }
            return string.Join(' ', tag);
        }

        /// <summary>
        /// 生成投稿人信息
        /// </summary>
        /// <param name="post"></param>
        /// <param name="poster"></param>
        /// <returns></returns>
        public string MakePoster(Posts post, Users poster)
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

        /// <summary>
        /// 生成稿件
        /// </summary>
        /// <param name="post"></param>
        /// <param name="poster"></param>
        /// <returns></returns>
        public string MakePostText(Posts post, Users poster)
        {
            var tag = TagsToString(post.Tags);

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

        /// <summary>
        /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
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
