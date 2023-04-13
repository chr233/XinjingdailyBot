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
        /// 添加媒体组消息
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task AddPostMediaGroup(IEnumerable<Message> messages);
    }
}
