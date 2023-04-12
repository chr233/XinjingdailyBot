using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface IDialogueService : IBaseService<Dialogue>
    {
        /// <summary>
        /// 记录群组聊天消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task RecordMessage(Message message);
    }
}
