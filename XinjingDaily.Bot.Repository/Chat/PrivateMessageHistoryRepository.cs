using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chats;
using XinjingDaily.Bot.IRepository.Chat;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Chat;

/// <summary>
/// 私聊消息历史仓储实现
/// </summary>
[RegisterScoped]
public class PrivateMessageHistoryRepository : RepositoryInt<PrivateMessageHistory>, IPrivateMessageHistoryRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public PrivateMessageHistoryRepository(ISqlSugarClient db) : base(db)
    {
    }
}
