using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Model.Models;
using XinjingdailyBot.Service.Data.Base;

namespace XinjingdailyBot.Service.Data
{
    [AppService(typeof(IDialogueService), LifeTime.Transient)]
    public sealed class DialogueService : BaseService<Dialogue>, IDialogueService
    {
        public async Task RecordMessage(Message message)
        {
            string? content = message.Type switch {
                MessageType.Text => message.Text,
                MessageType.Photo => message.Caption,
                MessageType.Audio => message.Caption,
                MessageType.Video => message.Caption,
                MessageType.Voice => message.Caption,
                MessageType.Document => message.Caption,
                MessageType.Sticker => message.Sticker!.SetName,
                _ => null,
            };

            if (content?.Length > 2000)
            {
                content = content[..2000];
            }

            var dialogue = new Dialogue {
                ChatID = message.Chat.Id,
                MessageID = message.MessageId,
                UserID = message.From?.Id ?? -1,
                ReplyMessageID = message.ReplyToMessage?.MessageId ?? -1,
                Type = message.Type.ToString(),
                Content = content ?? "",
                CreateAt = message.Date,
            };

            await InsertAsync(dialogue);
        }
    }
}
