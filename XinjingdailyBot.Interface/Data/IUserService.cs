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
    /// <summary>
    /// 更新用户投稿数据
    /// </summary>
    /// <param name="targerUser"></param>
    /// <returns></returns>
    Task UpdateUserPostCount(Users targerUser);
    /// <summary>
    /// 获取用户数量
    /// </summary>
    /// <returns></returns>
    Task<int> CountUser();
    /// <summary>
    /// 获取指定日期后有更新的用户数量
    /// </summary>
    /// <param name="afterDate"></param>
    /// <returns></returns>
    Task<int> CountRecentlyUpdateUser(DateTime afterDate);
    /// <summary>
    /// 获取未封禁用户
    /// </summary>
    /// <returns></returns>
    Task<int> CountUnBannedUser();
    /// <summary>
    /// 获取投稿通过的用户
    /// </summary>
    /// <returns></returns>
    Task<int> CountPostedUser();
    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    Task<List<Users>> GetUserList(IEnumerable<long> userIds);
    /// <summary>
    /// 获取startId后的用户列表
    /// </summary>
    /// <param name="startId"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    Task<List<Users>> GetUserListAfterId(int startId, int count);
    /// <summary>
    /// 获取通过数量排行
    /// </summary>
    /// <param name="miniumPost"></param>
    /// <param name="miniumPostTime"></param>
    /// <param name="takeCount"></param>
    /// <returns></returns>
    Task<List<Users>> GetUserAcceptCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount);
    /// <summary>
    /// 获取管理员通过排行
    /// </summary>
    /// <param name="miniumPost"></param>
    /// <param name="miniumPostTime"></param>
    /// <param name="takeCount"></param>
    /// <returns></returns>
    Task<List<Users>> GetAdminUserAcceptCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount);
    /// <summary>
    /// 获取管理员审核排行
    /// </summary>
    /// <param name="miniumPost"></param>
    /// <param name="miniumPostTime"></param>
    /// <param name="takeCount"></param>
    /// <returns></returns>
    Task<List<Users>> GetAdminUserReviewCountRankList(int miniumPost, DateTime miniumPostTime, int takeCount);
    /// <summary>
    /// 更新用户组
    /// </summary>
    /// <param name="user"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    Task UpdateUserGroupId(Users user, int groupId);
    /// <summary>
    /// 设置用户通知偏好
    /// </summary>
    /// <param name="user"></param>
    /// <param name="notification"></param>
    /// <returns></returns>
    Task SetUserNotification(Users user, bool notification);
    /// <summary>
    /// 设置默认匿名偏好
    /// </summary>
    /// <param name="user"></param>
    /// <param name="preferAnonymous"></param>
    /// <returns></returns>
    Task SetUserPreferAnonymous(Users user, bool preferAnonymous);
}
