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
}
