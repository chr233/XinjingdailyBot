using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 封禁记录仓储服务
/// </summary>
public interface IBanRecordService : IBaseService<BanRecords>
{
    /// <summary>
    /// 被警告超过此值自动封禁
    /// </summary>
    public const int WarningLimit = 3;

    /// <summary>
    /// 警告有效时间, 超过设定时间不计警告
    /// </summary>
    public const int WarnDuration = 90;

    /// <summary>
    /// 添加封禁记录
    /// </summary>
    /// <param name="targetUser"></param>
    /// <param name="operatorUser"></param>
    /// <param name="banType"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    Task AddBanRecord(Users targetUser, Users operatorUser, EBanType banType, string reason);
    /// <summary>
    /// 查询封禁记录
    /// </summary>
    /// <param name="targetUser"></param>
    /// <returns></returns>
    Task<List<BanRecords>> GetBanRecores(Users targetUser);
    /// <summary>
    /// 获取最近封禁/解封记录
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<BanRecords?> GetLatestBanRecord(long userId);

    /// <summary>
    /// 获取警告次数
    /// </summary>
    /// <param name="targetUser"></param>
    /// <returns></returns>
    Task<int> GetWarnCount(Users targetUser);
}
