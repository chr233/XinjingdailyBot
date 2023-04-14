using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IAttachmentService : IBaseService<Attachments>
    {
        /// <summary>
        /// 附件包装器
        /// </summary>
        /// <param name="message"></param>
        /// <param name="postID"></param>
        /// <returns></returns>
        Attachments? GenerateAttachment(Message message, long postID);
    }
}
