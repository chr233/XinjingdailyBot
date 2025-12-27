using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Interface.Bot;

public interface ITelegramBotService
{
    Task AutoReply(string text, CallbackQuery query, bool showAlert = false, CancellationToken cancellationToken = default);
    Task<Message> AutoReply(string text, Message message, ParseMode parsemode = ParseMode.None, CancellationToken cancellationToken = default);
    Task BanChatMember(ChatId chatId, long userId, DateTime? untilDate = null, bool revokeMessages = false, CancellationToken cancellationToken = default);
    Task<ChatInviteLink> CreateChatInviteLink(ChatId chatId, string? name = null, DateTime? expireDate = null, int? memberLimit = null, bool createsJoinRequest = false, CancellationToken cancellationToken = default);
    Task DeleteMessage(ChatId chatId, int messageId, CancellationToken cancellationToken = default);
    Task<Message> EditMessageReplyMarkup(Message message, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> EditMessageText(ChatId chatId, int messageId, string text, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> EditMessageText(Message message, string text, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<string> GetChatMemberStatus(Chat? chat, long userId);
    Task<TGFile> GetFile(string fileId, CancellationToken cancellationToken = default);
    Task LeaveChat(ChatId chatId, CancellationToken cancellationToken = default);
    Task PinChatMessage(ChatId chatId, int messageId, bool disableNotification = false, string? businessConnectionId = null, CancellationToken cancellationToken = default);
    Task<Message> RemoveMessageReplyMarkup(Message message, CancellationToken cancellationToken = default);
    Task RestrictChatMember(ChatId chatId, long userId, ChatPermissions permissions, bool useIndependentChatPermissions = false, DateTime? untilDate = null, CancellationToken cancellationToken = default);
    Task SendChatAction(Message message, ChatAction chatAction, CancellationToken cancellationToken = default);
    Task<Message> SendCommandReply(string text, Message message, bool? autoDelete = null, ParseMode parsemode = ParseMode.None, ReplyMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(ChatId chatId, string text, int? messageThreadId, int? replyToMessageId, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(long chatId, string text, int? messageThreadId=null, int? replyToMessageId = null, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(Message message, string text, ParseMode parseMode = ParseMode.None, bool replyToMessage = false, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendMessage(Chat chat, string text, int? messageThreadId, int? replyToMessageId, ParseMode parseMode = ParseMode.None, bool disableWebPagePreview = false, bool disableNotification = false, InlineKeyboardMarkup? replyMarkup = null, CancellationToken cancellationToken = default);
    Task<Message> SendPhoto(ChatId chatId, InputFile photo, string? caption = null, ParseMode parseMode = ParseMode.None, Message? replyMessage = null, ReplyMarkup? replyMarkup = null, int? messageThreadId = null, IEnumerable<MessageEntity>? captionEntities = null, bool showCaptionAboveMedia = false, bool hasSpoiler = false, bool disableNotification = false, bool protectContent = false, string? messageEffectId = null, string? businessConnectionId = null, bool allowPaidBroadcast = false, long? directMessagesTopicId = null, SuggestedPostParameters? suggestedPostParameters = null, CancellationToken cancellationToken = default);
    Task UnbanChatMember(ChatId chatId, long userId, bool onlyIfBanned = false, CancellationToken cancellationToken = default);
    Task UnpinAllChatMessages(ChatId chatId, CancellationToken cancellationToken = default);
    Task UnpinChatMessage(ChatId chatId, int? messageId = null, string? businessConnectionId = null, CancellationToken cancellationToken = default);
}