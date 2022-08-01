using System.Text;
using System.Text.RegularExpressions;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Helpers
{
    internal sealed class TextHelper
    {
        private static Regex MatchTag { get; } = new(@"(^#\S+)|(\s#\S+)");
        private static Regex MatchSpace { get; } = new(@"^\s*$");

        /// <summary>
        /// 去除无用文本内容
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PureText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            text = MatchTag.Replace(text, "");

            var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Where(x => !MatchSpace.IsMatch(x)).Select(x => x.Trim());

            return string.Join('\n', parts);
        }

        /// <summary>
        /// 提取Tag
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static BuildInTags FetchTags(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return BuildInTags.None;
            }

            BuildInTags tags = BuildInTags.None;

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
            return tags;
        }

        /// <summary>
        /// Html格式的用户链接
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static string HtmlUserLink(Users user)
        {
            string userNick = user.UserNick;
            return HtmlUserLink(user.UserID, user.UserName, userNick);
        }

        /// <summary>
        /// Html格式的用户链接
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userNick"></param>
        /// <returns></returns>
        internal static string HtmlUserLink(long userId, string userName, string userNick)
        {
            string nick = EscapeHtml(userNick);

            if (string.IsNullOrEmpty(userName))
            {
                return $"<a href=\"tg://user?id={userId}\">{nick}</a>";
            }
            else
            {
                return $"<a href=\"https://t.me/{userName}\">{nick}</a>";
            }
        }

        /// <summary>
        /// HTML格式的消息链接
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="chatName"></param>
        /// <param name="linkName"></param>
        /// <returns></returns>
        internal static string HtmlMessageLink(long messageID, string chatName, string linkName)
        {
            return $"<a href=\"https://t.me/{chatName}/{messageID}\">{linkName}</a>";
        }

        /// <summary>
        /// HTML转义
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string EscapeHtml(string text)
        {
            string escapedText = text.Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("&", "&amp;");
            return escapedText;
        }

        /// <summary>
        /// 生成审核消息(待审核)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        internal static string MakeReviewMessage(Users poster, bool anymouse)
        {
            string pUser = HtmlUserLink(poster);
            string strAny = anymouse ? "匿名投稿" : "保留来源";
            string status = "待审核";

            string msg = string.Join('\n', $"投稿人: {pUser}", "", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核通过)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        internal static string MakeReviewMessage(Users poster, Users reviewer, bool anymouse)
        {
            string pUser = HtmlUserLink(poster);
            string rUser = HtmlUserLink(reviewer);
            string strAny = anymouse ? "匿名投稿" : "保留来源";
            string status = "已发布";

            string msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核通过, 直接发布)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        internal static string MakeReviewMessage(Users poster, long messageID, bool anymouse)
        {
            string pUser = HtmlUserLink(poster);
            string msgLink = HtmlMessageLink(messageID, Utils.AcceptChannel.Username ?? Utils.AcceptChannel.Id.ToString(), "消息直链");
            string strAny = anymouse ? "匿名投稿" : "保留来源";
            string status = "已发布";

            string msg = string.Join('\n', $"发布人: {pUser}", $"消息: {msgLink}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 生成审核消息(审核未通过)
        /// </summary>
        /// <param name="poster"></param>
        /// <param name="reviewer"></param>
        /// <returns></returns>
        internal static string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason)
        {
            string pUser = HtmlUserLink(poster);
            string rUser = HtmlUserLink(reviewer);
            string strAny = anymouse ? "匿名投稿" : "保留来源";
            string status = $"已拒绝 {rejectReason}";

            string msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
            return msg;
        }

        /// <summary>
        /// 格式化RejectReason
        /// </summary>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        internal static string RejectReasonToString(RejectReason rejectReason)
        {
            string reason = rejectReason switch
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
        internal static string MakeNotification(bool isDirect)
        {
            return isDirect ? "稿件已发布" : "稿件已通过, 感谢您的支持";
        }

        /// <summary>
        /// 生成通知消息(审核未通过）
        /// </summary>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        internal static string MakeNotification(string reason)
        {
            string msg = string.Join('\n', "稿件未通过", $"原因: {reason}");
            return msg;
        }

        /// <summary>
        /// 格式化BuildInTags
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        internal static string TagsToString(BuildInTags tags)
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
            return string.Join(' ', tag);
        }

        /// <summary>
        /// 生成投稿人信息
        /// </summary>
        /// <param name="post"></param>
        /// <param name="poster"></param>
        /// <returns></returns>
        internal static string MakePoster(Posts post, Users poster)
        {
            string user = HtmlUserLink(poster);

            if (post.IsFromChannel)
            {
                string channel = HtmlUserLink(0, post.ChannelName, post.ChannelTitle);
                if (post.Anymouse)
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
                if (post.Anymouse)
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
        internal static string MakePostText(Posts post, Users poster)
        {
            string tag = TagsToString(post.Tags);

            StringBuilder sb = new();

            if (!string.IsNullOrEmpty(tag))
            {
                sb.AppendLine(tag);
            }

            if (!string.IsNullOrEmpty(post.Text))
            {
                string text = EscapeHtml(post.Text);
                sb.AppendLine(text);
            }

            string from = MakePoster(post, poster);

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.AppendLine(from);

            return sb.ToString();
        }
    }
}
