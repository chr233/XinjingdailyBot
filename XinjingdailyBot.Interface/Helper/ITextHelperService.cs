using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper;

/// <summary>
/// Text工具类服务
/// </summary>
public interface ITextHelperService
{
    /// <summary>
    /// HTML转义
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    string EscapeHtml(string? text);
    /// <summary>
    /// Html链接
    /// </summary>
    /// <param name="url"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    string HtmlLink(string url, string text);
    /// <summary>
    /// HTML格式的消息链接
    /// </summary>
    /// <param name="messageID"></param>
    /// <param name="chatName"></param>
    /// <param name="linkName"></param>
    /// <returns></returns>
    string HtmlMessageLink(long messageID, string chatName, string linkName);
    /// <summary>
    /// Html格式的用户链接
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    /// <param name="userNick"></param>
    /// <returns></returns>
    string HtmlUserLink(long userId, string userName, string userNick);
    /// <summary>
    /// Html格式的用户链接
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    string HtmlUserLink(Users user);
    /// <summary>
    /// 生成通知消息(审核通过）
    /// </summary>
    /// <param name="isDirect"></param>
    /// <param name="messageID"></param>
    /// <returns></returns>
    string MakeNotification(bool isDirect, long messageID);
    /// <summary>
    /// 生成通知消息(审核未通过）
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    string MakeNotification(string reason);
    /// <summary>
    /// 生成投稿人信息
    /// </summary>
    /// <param name="post"></param>
    /// <param name="poster"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    string MakePoster(NewPosts post, Users poster, ChannelOptions? channel);
    /// <summary>
    /// 生成稿件
    /// </summary>
    /// <param name="post"></param>
    /// <param name="poster"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    string MakePostText(NewPosts post, Users poster, ChannelOptions? channel);
    /// <summary>
    /// 生成审核消息(待审核)
    /// </summary>
    /// <param name="poster"></param>
    /// <param name="anymouse"></param>
    /// <returns></returns>
    string MakeReviewMessage(Users poster, bool anymouse);
    /// <summary>
    /// 生成审核消息(审核通过, 直接发布)
    /// </summary>
    /// <param name="poster"></param>
    /// <param name="messageID"></param>
    /// <param name="anymouse"></param>
    /// <returns></returns>
    string MakeReviewMessage(Users poster, long messageID, bool anymouse);
    /// <summary>
    /// 生成审核消息(审核通过)
    /// </summary>
    /// <param name="poster"></param>
    /// <param name="reviewer"></param>
    /// <param name="anymouse"></param>
    /// <returns></returns>
    string MakeReviewMessage(Users poster, Users reviewer, bool anymouse);
    /// <summary>
    /// 生成审核消息(审核未通过)
    /// </summary>
    /// <param name="poster"></param>
    /// <param name="reviewer"></param>
    /// <param name="anymouse"></param>
    /// <param name="rejectReason"></param>
    /// <returns></returns>
    string MakeReviewMessage(Users poster, Users reviewer, bool anymouse, string rejectReason);
    /// <summary>
    /// 根据Message.Enetities的字段格式生成HTML文本, 自动过滤无用HashTag
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    string ParseMessage(Message message);
    /// <summary>
    /// 去除所有HashTag和连续换行
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    string PureText(string? text);
}
