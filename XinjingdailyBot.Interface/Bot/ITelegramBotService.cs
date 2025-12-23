using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Interface.Bot;

public interface ITelegramBotService
{
    Task AutoReply(string text, CallbackQuery query, bool showAlert = false, CancellationToken cancellationToken = default);
    Task<Message> AutoReply(string text, Message message, ParseMode parsemode = ParseMode.None, CancellationToken cancellationToken = default);
    Task<Message> EditMessageReplyMarkup(Message message, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> EditMessageText(ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> EditMessageText(Message message, string text, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<string> GetChatMemberStatus(Chat? chat, long userId);
    Task<TGFile> GetFile(string fileId, CancellationToken cancellationToken = default);
    Task<Message> RemoveMessageReplyMarkup(Message message, CancellationToken cancellationToken = default);
    Task SendChatAction(Message message, ChatAction chatAction, CancellationToken cancellationToken = default);
    Task<Message> SendCommandReply(string text, Message message, bool? autoDelete = null, ParseMode parsemode = ParseMode.None, ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(ChatId chatId, string text, int? messageThreadId, int? replyToMessageId, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(long chatId, string text, int? messageThreadId, int? replyToMessageId, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessageEx(Message message, string text, ParseMode parseMode = ParseMode.None, bool replyToMessage = false, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
}