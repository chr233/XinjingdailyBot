using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IAttachmentService : IBaseService<Attachments>
    {
        Attachments? GenerateAttachment(Message message, long postID);
    }
}
