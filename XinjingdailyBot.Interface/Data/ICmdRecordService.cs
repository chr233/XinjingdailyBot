using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    public interface ICmdRecordService
    {
        Task AddCmdRecord(Message message, Users dbUser, string command, bool handled, bool isQuery, string? exception = null);
    }
}
