using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace XinjingDaily.Bot.Infrastructure.Extensions;

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
    public static Task<Message> AutoReply(
        this ITelegramBotClient botClient,
        string text,
        Message message,
        ParseMode parsemode = ParseMode.Markdown,
        CancellationToken cancellationToken = default)
    {
        return botClient.SendMessage(message.Chat, text, parseMode: parsemode, replyParameters: new ReplyParameters { MessageId = message.MessageId, AllowSendingWithoutReply = true }, cancellationToken: cancellationToken);
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
    public static Task<Message> EditMessageReplyMarkup(
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
    public static Task<Message> RemoveMessageReplyMarkup(
        this ITelegramBotClient botClient,
        Message message,
        CancellationToken cancellationToken = default)
    {
        return botClient.EditMessageReplyMarkup(message.Chat, message.MessageId, null, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 获取群成员状态
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="chat"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
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
