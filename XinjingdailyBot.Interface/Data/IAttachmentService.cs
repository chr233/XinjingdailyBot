using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data;

/// <summary>
/// 稿件附件仓储服务
/// </summary>
public interface IAttachmentService : IBaseService<Attachments>
{
    /// <summary>
    /// 创建单条附件
    /// </summary>
    /// <param name="attachment"></param>
    /// <returns></returns>
    Task CreateAttachment(Attachments attachment);
    Task<int> CreateAttachment(Message message, int postId);

    /// <summary>
    /// 创建多条附件
    /// </summary>
    /// <param name="attachments"></param>
    /// <returns></returns>
    Task CreateAttachments(List<Attachments> attachments);
    Task<int> CreateAttachments(Message[] messages, int postId);

    /// <summary>
    /// 根据稿件ID获取附件
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    Task<Attachments> FetchAttachmentByPostId(long postId);
    /// <summary>
    /// 根据稿件ID获取附件（多条）
    /// </summary>
    /// <param name="postId"></param>
    /// <returns></returns>
    Task<List<Attachments>> FetchAttachmentsByPostId(long postId);

    /// <summary>
    /// 附件包装器
    /// </summary>
    /// <param name="message"></param>
    /// <param name="postID"></param>
    /// <returns></returns>
    [Obsolete("过时方法")]
    Attachments? GenerateAttachment(Message message, long postID);
    /// <summary>
    /// 获取附件个数
    /// </summary>
    /// <returns></returns>
    Task<int> GetAttachmentCount();
}
