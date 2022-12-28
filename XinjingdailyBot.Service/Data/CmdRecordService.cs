using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(ServiceType = typeof(ICmdRecordService), ServiceLifetime = LifeTime.Transient)]
    public sealed class CmdRecordService : BaseService<CmdRecords>, ICmdRecordService
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
        public async Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception = null)
        {
            bool error = !string.IsNullOrEmpty(exception);

            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = message.Text ?? "",
                Handled = handled,
                IsQuery = isQuery,
                Error = error,
                Exception = exception ?? "",
                ExecuteAt = DateTime.Now,
            };

            await Insertable(record).ExecuteCommandAsync();
        }
    }
}
