using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Users;
using XinjingDaily.Bot.IRepository.User;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User;

/// <summary>
/// 用户统计仓储实现
/// </summary>
[RegisterScoped]
public class UserStatisticsRepository : RepositoryInt<UserStatistic>, IUserStatisticRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public UserStatisticsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
