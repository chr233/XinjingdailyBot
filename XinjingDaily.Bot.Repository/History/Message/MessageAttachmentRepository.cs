using SqlSugar;
using XinjingDaily.Bot.Entry.Entries.History.Message;
using XinjingDaily.Bot.IRepository.History.Message;
using XinjingDaily.Bot.Repository.Base;

namespace XinjingDaily.Bot.Repository.History.Message;

/// <summary>
/// 消息附件仓储实现
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="db"></param>
[RegisterScoped(Registration = RegistrationStrategy.ImplementedInterfaces)]
public class MessageAttachmentRepository(ISqlSugarClient db) : RepositoryInt<MessageAttachment>(db), IMessageAttachmentRepository
{
}
