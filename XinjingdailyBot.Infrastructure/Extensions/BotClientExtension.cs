using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// BotClient扩展
/// </summary>
public static class BotClientExtension
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 发送回复
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="text"></param>
    /// <param name="message"></param>
    /// <param name="parsemode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> AutoReply(
        this ITelegramBotClient botClient,
        string text,
        Message message,
        ParseMode parsemode = ParseMode.None,
        CancellationToken cancellationToken = default)
    {
        var replyParameters = new ReplyParameters {
            AllowSendingWithoutReply = true,
            MessageId = message.MessageId,
        };
        return botClient.SendMessage(message.Chat, text, parseMode: parsemode, replyParameters: replyParameters, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送回复
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="text"></param>
    /// <param name="query"></param>
    /// <param name="showAlert"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task AutoReply(
        this ITelegramBotClient botClient,
        string text,
        CallbackQuery query,
        bool showAlert = false,
        CancellationToken cancellationToken = default)
    {
        return botClient.AnswerCallbackQuery(query.Id, text, showAlert: showAlert, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 编辑消息Markup
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> EditMessageReplyMarkup
        (
        this ITelegramBotClient botClient,
        Message message,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        return botClient.EditMessageReplyMarkup(message.Chat, message.MessageId, replyMarkup, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 删除消息Markup
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> RemoveMessageReplyMarkup(
        this ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        return botClient.EditMessageReplyMarkup(message.Chat, message.MessageId, null, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 编辑消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="text"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="parseMode"></param>
    /// <param name="disableWebPagePreview"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> EditMessageText(
        this ITelegramBotClient botClient,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.None,
        bool disableWebPagePreview = false,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = disableWebPagePreview,
        };

        return botClient.EditMessageText(message.Chat, message.MessageId, text, parseMode: parseMode, replyMarkup: replyMarkup, linkPreviewOptions: linkPreviewOptions, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 编辑消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="messageId"></param>
    /// <param name="text"></param>
    /// <param name="parseMode"></param>
    /// <param name="disableWebPagePreview"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> EditMessageText(
        this ITelegramBotClient botClient,
        ChatId chatId,
        int messageId,
        string text,
        ParseMode parseMode = ParseMode.None,
        bool disableWebPagePreview = false,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = disableWebPagePreview,
        };

        return botClient.EditMessageText(chatId, messageId, text, parseMode: parseMode, replyMarkup: replyMarkup, linkPreviewOptions: linkPreviewOptions, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="text"></param>
    /// <param name="messageThreadId"></param>
    /// <param name="replyToMessageId"></param>
    /// <param name="parseMode"></param>
    /// <param name="disableWebPagePreview"></param>
    /// <param name="disableNotification"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> SendMessage(
        this ITelegramBotClient botClient,
        long chatId,
        string text,
        int? messageThreadId,
        int? replyToMessageId,
        ParseMode parseMode = ParseMode.None,
        bool disableWebPagePreview = false,
        bool disableNotification = false,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = disableWebPagePreview,
        };

        var replyParameters = replyToMessageId != null ? new ReplyParameters {
            AllowSendingWithoutReply = true,
            MessageId = replyToMessageId.Value,
        } : null;

        return botClient.SendMessage(chatId, text, parseMode: parseMode, messageThreadId: messageThreadId, replyMarkup: replyMarkup, linkPreviewOptions: linkPreviewOptions, replyParameters: replyParameters, disableNotification: disableNotification, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chatId"></param>
    /// <param name="text"></param>
    /// <param name="messageThreadId"></param>
    /// <param name="replyToMessageId"></param>
    /// <param name="parseMode"></param>
    /// <param name="disableWebPagePreview"></param>
    /// <param name="disableNotification"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> SendMessage(
        this ITelegramBotClient botClient,
        ChatId chatId,
        string text,
        int? messageThreadId,
        int? replyToMessageId,
        ParseMode parseMode = ParseMode.None,
        bool disableWebPagePreview = false,
        bool disableNotification = false,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = disableWebPagePreview,
        };

        var replyParameters = replyToMessageId != null ? new ReplyParameters {
            AllowSendingWithoutReply = true,
            MessageId = replyToMessageId.Value,
        } : null;

        return botClient.SendMessage(chatId, text, parseMode: parseMode, messageThreadId: messageThreadId, replyMarkup: replyMarkup, linkPreviewOptions: linkPreviewOptions, replyParameters: replyParameters, disableNotification: disableNotification, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="text"></param>
    /// <param name="parseMode"></param>
    /// <param name="replyToMessage"></param>
    /// <param name="disableWebPagePreview"></param>
    /// <param name="disableNotification"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task<Message> SendMessageEx(
        this ITelegramBotClient botClient,
        Message message,
        string text,
        ParseMode parseMode = ParseMode.None,
        bool replyToMessage = false,
        bool disableWebPagePreview = false,
        bool disableNotification = false,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = disableWebPagePreview,
        };

        var replyParameters = replyToMessage ? new ReplyParameters {
            AllowSendingWithoutReply = true,
            MessageId = message.MessageId,
        } : null;

        return botClient.SendMessage(message.Chat, text, parseMode: parseMode, messageThreadId: message.MessageThreadId, replyMarkup: replyMarkup, replyParameters: replyParameters, linkPreviewOptions: linkPreviewOptions, disableNotification: disableNotification, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送命令回复
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="text"></param>
    /// <param name="message"></param>
    /// <param name="autoDelete">私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除</param>
    /// <param name="parsemode"></param>
    /// <param name="replyMarkup"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static async Task<Message> SendCommandReply(
        this ITelegramBotClient botClient,
        string text,
        Message message,
        bool? autoDelete = null,
        ParseMode parsemode = ParseMode.None,
        ReplyMarkup? replyMarkup = null,
        CancellationToken cancellationToken = default)
    {
        //私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除
        bool delete = (autoDelete != null ? autoDelete.Value : (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)) && message.Chat.Type != ChatType.Private;

        var replyParameters = new ReplyParameters {
            AllowSendingWithoutReply = true,
            MessageId = message.MessageId,
        };
        var linkPreviewOptions = new LinkPreviewOptions {
            IsDisabled = true,
        };

        var msg = await botClient.SendMessage(message.Chat, text, parseMode: parsemode, messageThreadId: message.MessageThreadId,
            replyParameters: replyParameters, replyMarkup: replyMarkup, linkPreviewOptions: linkPreviewOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (delete)
        {
            _ = Task.Run(async () => {
                await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                try
                {
                    await botClient.DeleteMessage(msg.Chat, msg.MessageId, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    _logger.Error("删除消息 {messageId} 失败", msg.MessageId);
                }
            }, cancellationToken);
        }

        return msg;
    }

    /// <summary>
    /// 发送会话状态
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="chatAction"></param>
    /// <param name="threadId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Obsolete]
    public static Task SendChatAction(
        this ITelegramBotClient botClient,
        Message message,
        ChatAction chatAction,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return botClient.SendChatAction(message.Chat, chatAction, message.MessageThreadId, null, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "SendChatAction出错");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 获取群成员状态
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chat"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    [Obsolete]
    public static async Task<string> GetChatMemberStatus(
        this ITelegramBotClient botClient,
        Chat? chat,
        long userId)
    {
        string title = chat?.Title?.EscapeHtml() ?? "未设置群组";

        string status;
        if (chat != null)
        {
            try
            {
                var chatMember = await botClient.GetChatMember(chat, userId).ConfigureAwait(false);
                status = chatMember.Status switch {
                    ChatMemberStatus.Creator or
                    ChatMemberStatus.Administrator or
                    ChatMemberStatus.Member or
                    ChatMemberStatus.Left => "正常",
                    ChatMemberStatus.Kicked => "封禁",
                    ChatMemberStatus.Restricted => "受限",
                    _ => "未知",
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "读取用户状态出错");
                status = "出错";
            }
        }
        else
        {
            status = "---";
        }

        return string.Format("{0}: {1}", title, status);
    }
}
