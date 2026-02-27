using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chats;
using XinjingDaily.Bot.IRepository.Chat;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Chat;

/// <summary>
/// 聊天信息仓储实现
/// </summary>
[RegisterScoped]
public class ChatInfosRepository : RepositoryInt<GroupInfos>, IChatInfosRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChatInfosRepository(ISqlSugarClient db) : base(db)
    {
    }
}
