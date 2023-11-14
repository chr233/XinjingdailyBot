using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 用户仓储服务
/// </summary>
public interface IUserService : IBaseService<Users>
{
    /// <summary>
    /// 根据ReplyToMessage获取目标用户
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<Users?> FetchTargetUser(Message message);
    /// <summary>
    /// 根据UserID获取用户
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    Task<Users?> FetchUserByUserID(long userID);
    /// <summary>
    /// 根据UserName获取用户
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    Task<Users?> FetchUserByUserName(string? userName);
    /// <summary>
    /// 根据用户输入查找指定用户
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    Task<Users?> FetchUserByUserNameOrUserID(string? target);
    /// <summary>
    /// 根据Update获取发送消息的用户
    /// </summary>
    /// <param name="update"></param>
    /// <returns></returns>
    Task<Users?> FetchUserFromUpdate(Update update);
    /// <summary>
    /// 获取用户基本信息
    /// </summary>
    /// <param name="dbUser"></param>
    /// <returns></returns>
    string GetUserBasicInfo(Users dbUser);
    /// <summary>
    /// 获取用户排名
    /// </summary>
    /// <param name="dbUser"></param>
    /// <returns></returns>
    Task<string> GetUserRank(Users dbUser);
    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="UserId"></param>
    /// <returns></returns>
    Task<Users?> QueryUserByUserId(long UserId);
    /// <summary>
    /// 查找全部用户
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    Task<(string, InlineKeyboardMarkup?)> QueryAllUserList(Users dbUser, string query, int page);
    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    Task<(string, InlineKeyboardMarkup?)> QueryUserList(Users dbUser, string query, int page);
    /// <summary>
    /// 查找用户
    /// </summary>
    /// <param name="chatId"></param>
    /// <param name="msgId"></param>
    /// <returns></returns>
    Task<Users?> FetchTargetUser(long chatId, int msgId);
    /// <summary>
    /// 封禁用户
    /// </summary>
    /// <param name="targetUser"></param>
    /// <param name="isBan"></param>
    /// <returns></returns>
    Task BanUser(Users targetUser, bool isBan);
    Task UpdateUserPostCount(Users targerUser);
}
