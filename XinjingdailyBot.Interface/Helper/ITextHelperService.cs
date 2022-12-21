using Telegram.Bot.Types;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface ITextHelperService
    {
        string EscapeHtml(string? text);
        BuildInTags FetchTags(string? text);
        string HtmlMessageLink(long messageID, string chatName, string linkName);
        string HtmlUserLink(long userId, string userName, string userNick);
        string HtmlUserLink(Users user);
        string MakeNotification(bool isDirect, long messageID);
        string MakeNotification(string reason);
        string MakePoster(Posts post, Users poster);
        string MakePostText(Posts post, Users poster);
        string MakeReviewMessage(Users poster, bool anymouse);
        string MakeReviewMessage(Users poster, long messageID, bool anymouse);
        string MakeReviewMessage(Users poster, Users reviewer, bool anymouse);
        string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason);
        string ParseMessage(Message message);
        string PureText(string? text);
        string RejectReasonToString(RejectReason rejectReason);
        string TagsToString(BuildInTags tags);
    }
}
