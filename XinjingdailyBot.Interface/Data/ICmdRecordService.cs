using Telegram.Bot.Types;
using XinjingdailyBot.Interface.Data.Base;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    /// <summary>
    /// 命令调用记录服务
    /// </summary>
    public interface ICmdRecordService : IBaseService<CmdRecords>
    {
        /// <summary>
        /// 新增命令调用记录
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dbUser"></param>
        /// <param name="handled"></param>
        /// <param name="isQuery"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception);
        /// <summary>
        /// 新增命令调用记录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dbUser"></param>
        /// <param name="handled"></param>
        /// <param name="isQuery"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        Task AddCmdRecord(CallbackQuery query, Users dbUser, bool handled, bool isQuery, string? exception);
    }
}
