using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chats;
using XinjingDaily.Bot.IRepository.Chat;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Chat;

/// <summary>
/// 群消息历史仓储实现
/// </summary>
[RegisterScoped]
public class GroupMessageHistoryRepository : RepositoryInt<GroupMessageHistory>, IGroupMessageHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public GroupMessageHistoryRepository(ISqlSugarClient db) : base(db)
    {
    }
}
