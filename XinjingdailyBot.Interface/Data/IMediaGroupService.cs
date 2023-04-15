using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    /// <summary>
    /// 媒体组消息服务
    /// </summary>
    public interface IMediaGroupService
    {
        /// <summary>
        /// 批量添加媒体组消息
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task AddPostMediaGroup(IEnumerable<Message> messages);
        /// <summary>
        /// 添加媒体组消息
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task AddPostMediaGroup(Message message);
        /// <summary>
        /// 查询媒体组消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<MediaGroups?> QueryMediaGroup(Message message);
        /// <summary>
        /// 查询媒体组消息
        /// </summary>
        /// <param name="mediaGroupId"></param>
        /// <returns></returns>
        Task<List<MediaGroups>> QueryMediaGroup(string? mediaGroupId);
        /// <summary>
        /// 查询媒体组消息
        /// </summary>
        /// <param name="chat"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        Task<MediaGroups?> QueryMediaGroup(Chat chat, long msgId);
    }
}
