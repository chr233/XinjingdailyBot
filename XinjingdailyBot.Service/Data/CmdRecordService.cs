using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(ICmdRecordService), LifeTime.Transient)]
    public sealed class CmdRecordService : BaseService<CmdRecords>, ICmdRecordService
    {
        public async Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception)
        {
            bool error = !string.IsNullOrEmpty(exception);

            string text = message.Text ?? "NULL";
            if (text.Length > 1000)
            {
                text = text[..1000];
            }

            CmdRecords record = new() {
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

        public async Task AddCmdRecord(CallbackQuery query, Users dbUser, bool handled, bool isQuery, string? exception)
        {
            bool error = !string.IsNullOrEmpty(exception);

            string text = query.Data ?? "NULL";
            if (text.Length > 1000)
            {
                text = text[..1000];
            }

            var message = query.Message!;
            CmdRecords record = new() {
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
