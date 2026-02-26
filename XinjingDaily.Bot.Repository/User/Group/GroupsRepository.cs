using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.User.Group;
using XinjingDaily.Bot.IRepository.User.Group;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.User.Group;

/// <summary>
/// 群组仓储实现
/// </summary>
[RegisterScoped]
public class GroupsRepository : RepositoryInt<Groups>, IGroupsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public GroupsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
