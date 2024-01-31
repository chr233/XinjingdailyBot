using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Infrastructure.Model;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Repository;

namespace XinjingdailyBot.Service.Helper;

/// <inheritdoc cref="ITextHelperService"/>
[AppService(typeof(ITextHelperService), LifeTime.Transient)]
public sealed class TextHelperService(
        IOptions<OptionsSetting> _options,
        TagRepository _tagRepository) : ITextHelperService
{
    /// <inheritdoc/>
    public string PureText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var option = _options.Value.Post;
        if (option.PureHashTag)
        {
            //过滤HashTag
            text = RegexUtils.MatchHashTag().Replace(text, "");
        }

        if (option.PureReturns)
        {
            var matchSpace = RegexUtils.MatchBlankLine();
            //过滤连续换行
            var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Where(x => !matchSpace.IsMatch(x)).Select(x => x.Trim());
            text = string.Join('\n', parts);
        }

        return text;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public string HtmlLink(string url, string text)
    {
        return $"<a href=\"{url}\">{text}</a>";
    }

    /// <inheritdoc/>
    public string HtmlUserLink(Users user)
    {
        return HtmlUserLink(user.UserID, user.UserName, user.FullName);
    }

    /// <inheritdoc/>
    public string HtmlMessageLink(long messageID, string chatName, string linkName)
    {
        return $"<a href=\"https://t.me/{chatName}/{messageID}\">{linkName}</a>";
    }
    
    public string HtmlMessageLink(long messageID, long chatId, string linkName)
    {
        if (chatId < 0)
        {
            chatId = -1000000000000L - chatId;
        }
        return $"<a href=\"https://t.me/c/{chatId}/{messageID}\">{linkName}</a>";
    }

    /// <inheritdoc/>
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

            foreach (var item in _options.Value.Post.PureWordsList)
            {
                escapedText = escapedText.Replace(item, "");
            }
            return escapedText;
        }
    }

    /// <inheritdoc/>
    public string MakeReviewMessage(Users poster, bool anymouse)
    {
        var pUser = HtmlUserLink(poster);
        var strAny = anymouse ? "匿名投稿" : "保留来源";
        var status = "待审核";

        var msg = string.Join('\n', $"#待审核 ", $"投稿人: {pUser}", "", $"模式: {strAny}", $"状态: {status}");
        return msg;
    }

    /// <inheritdoc/>
    public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, bool second, Message? message)
    {
        var pUser = HtmlUserLink(poster);
        var rUser = HtmlUserLink(reviewer);
        var msgLink = message != null ? HtmlLink(message.GetMessageLink(), "消息直链") : "无";
        var strAny = anymouse ? "匿名投稿" : "保留来源";
        var status = !second ? "已发布" : "已发布 (第二频道)";

        var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"消息: {msgLink}", $"模式: {strAny}", $"状态: {status}");
        return msg;
    }

    /// <inheritdoc/>
    public string MakeReviewMessage(Users poster, bool anymouse, bool second, Message? message)
    {
        var pUser = HtmlUserLink(poster);
        var strAny = anymouse ? "匿名投稿" : "保留来源";
        var status = !second ? "已发布" : "已发布 (第二频道)";
        var msgLink = message != null ? HtmlLink(message.GetMessageLink(), "消息直链") : "无";

        var msg = string.Join('\n', $"发布人: {pUser}", $"消息: {msgLink}", $"模式: {strAny}", $"状态: {status}");
        return msg;
    }

    /// <inheritdoc/>
    public string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason)
    {
        var pUser = HtmlUserLink(poster);
        var rUser = HtmlUserLink(reviewer);
        var strAny = anymouse ? "匿名投稿" : "保留来源";
        var status = $"已拒绝 {rejectReason}";

        var msg = string.Join('\n', $"投稿人: {pUser}", $"审核人: {rUser}", $"模式: {strAny}", $"状态: {status}");
        return msg;
    }

    /// <inheritdoc/>
    public string MakeNotification(bool isDirect, bool inPlan, Message? message)
    {
        var msgLink = message != null ? HtmlLink(message.GetMessageLink(), "消息直链") : "无";

        if (!inPlan)
        {
            return isDirect ? $"稿件已发布, {msgLink}" : $"稿件已通过, 感谢您的支持 {msgLink}";
        }
        else
        {
            return isDirect ? $"稿件将按设定频率定期发布, {msgLink}" : $"稿件已通过, 将设定频率定期发布, 感谢您的支持 {msgLink}";
        }
    }

    /// <inheritdoc/>
    public string MakeNotification(string reason)
    {
        var msg = string.Join('\n', "稿件未通过", $"原因: {reason}");
        return msg;
    }

    /// <inheritdoc/>
    public string MakePoster(NewPosts post, Users poster, ChannelOptions? channel)
    {
        var user = HtmlUserLink(poster);

        if (post.IsFromChannel && !string.IsNullOrEmpty(channel?.ChannelName))
        {
            var link = HtmlMessageLink(post.ChannelMsgID, channel.ChannelName, channel.ChannelTitle);
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
            if (post.Anonymous || post.ForceAnonymous)
            {
                return "<i>via</i> 匿名";
            }
            else
            {
                return $"<i>via</i> {user}";
            }
        }
    }

    /// <inheritdoc/>
    public string MakePostText(NewPosts post, Users poster, ChannelOptions? channel)
    {
        var tag = _tagRepository.GetActiviedHashTags(post.Tags);

        var sb = new StringBuilder();

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
    
    /// <param name="admin">操作管理员</param>
    /// <param name="target">目标用户</param>
    /// <param name="type">封禁类型</param>
    /// <returns></returns>
    public string MakeAdminLogText(Users admin, Users target, EBanType type, String reason , Message? responseMessage)
    {
        var sb = new StringBuilder();
        switch (type)
        {
            case EBanType.Ban:
            case EBanType.GlobalBan:
                sb.AppendLine("#BAN");
                break;
            case EBanType.GlobalMute:
                sb.AppendLine("#MUTE");
                break;
            case EBanType.UnBan: 
            case EBanType.GlobalUnBan: 
                sb.AppendLine("#UNBAN");
                break;
            case EBanType.GlobalUnMute:
                sb.AppendLine("#UNMUTE");
                break;
            case EBanType.Warning: 
                sb.AppendLine("#WARN");
                break;
        }

        sb.AppendLine($"<b>User</b>: {HtmlUserLink(target)} (<code>{target.UserID}</code>)");
        sb.AppendLine($"<b>Admin</b>: {HtmlUserLink(admin)} (<code>{admin.UserID}</code>)");
        sb.AppendLine($"<b>Reason</b>: {reason}");
        if (responseMessage != null)
        {
            sb.AppendLine($"<b>Message Link</b>: {HtmlMessageLink(responseMessage.MessageId, responseMessage.Chat.Id, "link")}");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
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

            if (tagMap.TryGetValue(start, out var tagStart))
            {
                tagStart.AddLast(head);
            }
            else
            {
                tagMap.Add(start, new TagObjct(head));
            }

            if (tagMap.TryGetValue(end, out var tagEnd))
            {
                tagEnd.AddFirst(tail);
            }
            else
            {
                tagMap.Add(end, new TagObjct(tail));
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
