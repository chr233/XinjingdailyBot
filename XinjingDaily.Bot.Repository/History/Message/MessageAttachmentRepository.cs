using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Message;
using XinjingDaily.Bot.IRepository.History.Message;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Message;

/// <summary>
/// 消息附件仓储实现
/// </summary>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class MessageAttachmentRepository : RepositoryInt<MessageAttachment>, IMessageAttachmentRepository
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="db"></param>
    public MessageAttachmentRepository(ISqlSugarClient db) : base(db)
    {
    }
}
