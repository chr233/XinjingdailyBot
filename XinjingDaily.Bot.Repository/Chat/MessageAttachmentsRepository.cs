using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.Chats;
using XinjingDaily.Bot.IRepository.Chat;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.Chat;

/// <summary>
/// 消息附件仓储实现
/// </summary>
[RegisterScoped]
public class MessageAttachmentsRepository : RepositoryInt<MessageAttachment>, IMessageAttachmentsRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public MessageAttachmentsRepository(ISqlSugarClient db) : base(db)
    {
    }
}
