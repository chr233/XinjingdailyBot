using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chats;
using XinjingDaily.Bot.IRepository.Chat;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Chat;

/// <summary>
/// 聊天信息仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class ChatInfoRepository : RepositoryInt<GroupInfo>, IChatInfoRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public ChatInfoRepository(ISqlSugarClient db) : base(db)
    {
    }
}
