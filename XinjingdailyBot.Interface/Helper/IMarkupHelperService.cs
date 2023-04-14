using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IMarkupHelperService
    {
        /// <summary>
        /// 直接发布投稿键盘
        /// </summary>
        /// <param name="anymouse"></param>
        /// <param name="tagNum"></param>
        /// <param name="hasSpoiler"></param>
        /// <returns></returns>
        InlineKeyboardMarkup DirectPostKeyboard(bool anymouse, int tagNum, bool? hasSpoiler);
        /// <summary>
        /// 跳转链接键盘
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        InlineKeyboardMarkup? LinkToOriginPostKeyboard(Posts post);
        /// <summary>
        /// 跳转链接键盘
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        InlineKeyboardMarkup? LinkToOriginPostKeyboard(long messageId);
        /// <summary>
        /// 投稿键盘
        /// </summary>
        /// <param name="anymouse"></param>
        /// <returns></returns>
        InlineKeyboardMarkup PostKeyboard(bool anymouse);
        /// <summary>
        /// 查询稿件信息
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        InlineKeyboardMarkup QueryPostMenuKeyboard(Users dbUser, Posts post);
        /// <summary>
        /// 获取随机投稿键盘
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser);
        /// <summary>
        /// 获取随机投稿键盘
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, int tagNum);
        /// <summary>
        /// 获取随机投稿键盘
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, Posts post, int tagId, string postType);
        /// <summary>
        /// 审核键盘A(选择稿件Tag)
        /// </summary>
        /// <param name="tagNum"></param>
        /// <param name="hasSpoiler"></param>
        /// <returns></returns>
        InlineKeyboardMarkup ReviewKeyboardA(int tagNum, bool? hasSpoiler);
        /// <summary>
        /// 审核键盘B(选择拒绝理由)
        /// </summary>
        /// <returns></returns>
        InlineKeyboardMarkup ReviewKeyboardB();
        /// <summary>
        /// 频道选项键盘
        /// </summary>
        /// <param name="channelOption"></param>
        /// <returns></returns>
        InlineKeyboardMarkup? SetChannelOptionKeyboard(Users dbUser, long channelId);
        /// <summary>
        /// 设置用户群组键盘
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="targetUser"></param>
        /// <returns></returns>
        Task<InlineKeyboardMarkup?> SetUserGroupKeyboard(Users dbUser, Users targetUser);
        /// <summary>
        /// 生成用户列表键盘
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="current">当前页码</param>
        /// <param name="total">总页码</param>
        /// <returns></returns>
        InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total);
    }
}
