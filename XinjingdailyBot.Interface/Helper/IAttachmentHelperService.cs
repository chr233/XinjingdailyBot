using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Helper
{
    public interface IAttachmentHelperService
    {
        Attachments? GenerateAttachment(Message message, long postID);
    }
}
