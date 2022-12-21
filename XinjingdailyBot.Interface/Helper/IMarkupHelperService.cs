using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Model.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IMarkupHelperService
    {
        InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, BuildInTags tag);
        InlineKeyboardMarkup PostKeyboard(bool anymouse);
        InlineKeyboardMarkup ReviewKeyboardA(BuildInTags tag);
        InlineKeyboardMarkup ReviewKeyboardB();
        Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser);
        InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total);
    }
}
