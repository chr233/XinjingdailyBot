using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(ICmdRecordService), LifeTime.Transient)]
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
        public async Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception)
        {
            bool error = !string.IsNullOrEmpty(exception);

            string text = message.Text ?? "NULL";
            if (text.Length > 1000)
            {
                text = text[..1000];
            }

            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = text,
                Handled = handled,
                IsQuery = isQuery,
                Error = error,
                Exception = exception ?? "",
                ExecuteAt = DateTime.Now,
            };

            await Insertable(record).ExecuteCommandAsync();
        }

        /// <summary>
        /// 新增命令调用记录
        /// </summary>
        /// <param name="query"></param>
        /// <param name="dbUser"></param>
        /// <param name="handled"></param>
        /// <param name="isQuery"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public async Task AddCmdRecord(CallbackQuery query, Users dbUser, bool handled, bool isQuery, string? exception)
        {
            bool error = !string.IsNullOrEmpty(exception);

            string text = query.Data ?? "NULL";
            if (text.Length > 1000)
            {
                text = text[..1000];
            }

            var message = query.Message!;
            CmdRecords record = new()
            {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = dbUser.UserID,
                Command = text,
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
