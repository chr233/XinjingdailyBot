using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 稿件附件仓储服务
/// </summary>
public interface IAttachmentService : IBaseService<Attachments>
{
    Task CreateAttachment(Attachments attachment);
    Task CreateAttachments(List<Attachments> attachments);
    Task<Attachments> FetchAttachmentByPostId(long postId);
    Task<List<Attachments>> FetchAttachmentsByPostId(long postId);

    /// <summary>
    /// 附件包装器
    /// </summary>
    /// <param name="message"></param>
    /// <param name="postID"></param>
    /// <returns></returns>
    Attachments? GenerateAttachment(Message message, long postID);
    Task<int> GetAttachmentCount();
}
