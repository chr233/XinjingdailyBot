using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot.Helpers
{
    internal static class SendMsgHelper
    {

        /// <summary>
        /// 发送命令回复
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="text"></param>
        /// <param name="message"></param>
        /// <param name="autoDelete">私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除</param>
        /// <param name="parsemode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal static async Task<Message> SendCommandReply(
            this ITelegramBotClient botClient,
            string text,
            Message message,
            bool? autoDelete = null,
            ParseMode? parsemode = null,
            CancellationToken cancellationToken = default)
        {
            //私聊始终不删除消息, 群聊中默认删除消息, 但可以指定不删除
            bool delete = (autoDelete != null ? autoDelete.Value : (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)) && message.Chat.Type != ChatType.Private;

            var msg = await botClient.SendTextMessageAsync(message.Chat.Id, text, parsemode, replyToMessageId: message.MessageId, allowSendingWithoutReply: true, cancellationToken: cancellationToken);

            if (delete)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    try
                    {
                        await botClient.DeleteMessageAsync(msg.Chat.Id, msg.MessageId, cancellationToken);
                    }
                    catch
                    {
                        Logger.Error($"删除消息 {msg.MessageId} 失败");
                    }
                }, cancellationToken);
            }

            return msg;
        }
    }
}
