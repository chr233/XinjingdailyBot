using Telegram.Bot.Types;
using XinjingDaily.Bot.Entry.Entries.Channel;
using XinjingDaily.Bot.Entry.Entries.Posts;
using XinjingDaily.Bot.Entry.Entries.Users;

namespace XinjingdailyBot.Service.Helper;

public interface ITextService
{
    string EscapeHtml(string? text);
    string HtmlLink(string url, string text);
    string HtmlMessageLink(long messageID, long chatId, string linkName);
    string HtmlMessageLink(long messageID, string chatName, string linkName);
    string HtmlUserLink(long userId, string userName, string userNick);
    string HtmlUserLink(UserInfo user);
    string MakeNotification(bool isDirect, bool inPlan, Message? message);
    string MakeNotification(string reason);
    string MakePoster(PostInfo post, UserInfo poster, SourceChannelSetting? channel);
    string MakePostText(PostInfo post, UserInfo poster, SourceChannelSetting? channel);
    string MakeReviewMessage(UserInfo poster, bool anymouse);
    string MakeReviewMessage(UserInfo poster, bool anymouse, bool second, Message? message);
    string MakeReviewMessage(UserInfo poster, UserInfo reviewer, bool anymouse, bool second, Message? message);
    string MakeReviewMessage(UserInfo poster, UserInfo reviewer, bool anymouse, string rejectReason);
    string ParseMessage(Message message);
    string PureText(string? text);
}