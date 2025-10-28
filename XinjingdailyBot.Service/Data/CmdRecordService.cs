using SqlSugar;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data;

/// <inheritdoc cref="ICmdRecordService"/>
[AppService(typeof(ICmdRecordService), LifeTime.Transient)]
public sealed class CmdRecordService : BaseService<CmdRecords>, ICmdRecordService
{
    public CmdRecordService(ISqlSugarClient context) : base(context)
    {
    }

    public async Task AddCmdRecord(Message message, Users dbUser, bool handled, bool isQuery, string? exception)
    {
        bool error = !string.IsNullOrEmpty(exception);

        string text = message.Text ?? "NULL";
        if (text.Length > 1000)
        {
            text = text[..1000];
        }

        var record = new CmdRecords {
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
        var record = new CmdRecords {
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

    public Task<CmdRecords> FetchCmdRecordByMessageId(int msgId)
    {
        return Queryable().FirstAsync(x => x.MessageID == msgId);
    }

    public Task<int> GetErrorCmdCount(DateTime startTime)
    {
        return Queryable().Where(x => x.Error && x.Handled && x.ExecuteAt >= startTime).CountAsync();
    }

    public Task<int> GetQueryCmdCount(DateTime startTime)
    {
        return Queryable().Where(x => x.IsQuery && x.Handled && x.ExecuteAt >= startTime).CountAsync();
    }

    public Task<int> GetTextCmdCount(DateTime startTime)
    {
        return Queryable().Where(x => !x.IsQuery && x.Handled && x.ExecuteAt >= startTime).CountAsync();
    }
}
