using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IMarkupHelperService
    {
        InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, BuildInTags tag);
        InlineKeyboardMarkup DirectPostKeyboardWithSpoiler(bool anymouse, BuildInTags tag);
        InlineKeyboardMarkup? LinkToOriginPostKeyboard(Posts post);
        InlineKeyboardMarkup PostKeyboard(bool anymouse);
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser);
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, Posts post, string tag);
        InlineKeyboardMarkup ReviewKeyboardA(BuildInTags tag);
        InlineKeyboardMarkup ReviewKeyboardAWithSpoiler(BuildInTags tag);
        InlineKeyboardMarkup ReviewKeyboardB();
        InlineKeyboardMarkup? SetChannelOptionKeyboard(Users dbUser, long channelId);
        Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser);
        InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total);
    }
}
