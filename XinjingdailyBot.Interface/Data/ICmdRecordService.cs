using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface ICmdRecordService : IBaseService<CmdRecords>
    {
        Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception);
        Task AddCmdRecord(CallbackQuery query, Users dbUser, bool handled, bool isQuery, string? exception);
    }
}
