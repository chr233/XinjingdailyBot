using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IMarkupHelperService
    {
        InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, int tagNum, bool? hasSpoiler);
        InlineKeyboardMarkup? LinkToOriginPostKeyboard(Posts post);
        InlineKeyboardMarkup? LinkToOriginPostKeyboard(string link);
        InlineKeyboardMarkup PostKeyboard(bool anymouse);
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser);
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, Posts post, string tagName, string tag);
        InlineKeyboardMarkup ReviewKeyboardA(int tagNum, bool? hasSpoiler);
        InlineKeyboardMarkup ReviewKeyboardB();
        InlineKeyboardMarkup? SetChannelOptionKeyboard(Users dbUser, long channelId);
        Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser);
        InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total);
    }
}
