using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper;

/// <summary>
/// 消息装饰器工具类服务
/// </summary>
public interface IMarkupHelperService
{
    /// <summary>
    /// 一行显示的字数
    /// </summary>
    public const int MaxLineCharsTag = 10;
    /// <summary>
    /// 一行显示的字数
    /// </summary>
    public const int MaxLineCharsReason = 8;

    /// <summary>
    /// 广告按钮设置
    /// </summary>
    /// <param name="externalLink"></param>
    /// <param name="extrnalLinkName"></param>
    /// <returns></returns>
    InlineKeyboardMarkup? AdvertiseExternalLinkButton(string? externalLink, string? extrnalLinkName);

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
    InlineKeyboardMarkup? LinkToOriginPostKeyboard(NewPosts post);
    /// <summary>
    /// 跳转链接键盘
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    InlineKeyboardMarkup? LinkToOriginPostKeyboard(long messageId);
    /// <summary>
    /// Nuke命令菜单键盘
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="targetUser"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    InlineKeyboardMarkup NukeMenuKeyboard(Users dbUser, Users targetUser, string reason);
    /// <summary>
    /// 投稿键盘
    /// </summary>
    /// <param name="anymouse"></param>
    /// <returns></returns>
    InlineKeyboardMarkup PostKeyboard(bool anymouse);
    /// <summary>
    /// 投稿警告键盘
    /// </summary>
    /// <param name="isDirectPost"></param>
    /// <returns></returns>
    InlineKeyboardMarkup PostWarningKeyboard(bool isDirectPost);
    /// <summary>
    /// 查询稿件信息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="post"></param>
    /// <returns></returns>
    InlineKeyboardMarkup QueryPostMenuKeyboard(Users dbUser, NewPosts post);
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
    InlineKeyboardMarkup RandomPostMenuKeyboard(Users dbUser, NewPosts post, int tagId, string postType);
    /// <summary>
    /// 审核键盘A(选择稿件Tag)
    /// </summary>
    /// <param name="tagNum"></param>
    /// <param name="hasSpoiler"></param>
    /// <param name="anonymous"></param>
    /// <returns></returns>
    InlineKeyboardMarkup ReviewKeyboardA(int tagNum, bool? hasSpoiler, bool? anonymous);
    /// <summary>
    /// 审核键盘B(选择拒绝理由)
    /// </summary>
    /// <returns></returns>
    InlineKeyboardMarkup ReviewKeyboardB();
    /// <summary>
    /// 获取指向今日最早未审核稿件的链接
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    InlineKeyboardMarkup ReviewStatusButton(NewPosts? post);

    /// <summary>
    /// 频道选项键盘
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="channelId"></param>
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
    /// <param name="query">参数</param>
    /// <param name="current">当前页码</param>
    /// <param name="total">总页码</param>
    /// <returns></returns>
    InlineKeyboardMarkup? UserListPageKeyboard(Users dbUser, string query, int current, int total);
}
