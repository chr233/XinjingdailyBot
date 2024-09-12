using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Service;

[AppService(LifeTime.Singleton)]
public class UpdateHandler(ILogger<UpdateHandler> logger) : IUpdateHandler
{
    private static readonly InputPollOption[] PollOptions = ["Hello", "World!"];

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch {
            { Message: { } message } => OnMessage(botClient, message),
            { EditedMessage: { } message } => OnMessage(botClient, message),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, callbackQuery),
            { InlineQuery: { } inlineQuery } => OnInlineQuery(botClient, inlineQuery),
            { ChosenInlineResult: { } chosenInlineResult } => OnChosenInlineResult(botClient, chosenInlineResult),
            { Poll: { } poll } => OnPoll(botClient, poll),
            { PollAnswer: { } pollAnswer } => OnPollAnswer(botClient, pollAnswer),

            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            _ => UnknownUpdateHandlerAsync(botClient, update)
        });
    }

    private async Task OnMessage(ITelegramBotClient botClient, Message msg)
    {
        logger.LogInformation("{bot} Receive message type: {MessageType}", botClient.BotId, msg.Type);
        if (msg.Text is not { } messageText)
            return;

        Message sentMessage = await (messageText.Split(' ')[0] switch {
            "/photo" => SendPhoto(botClient, msg),
            "/inline_buttons" => SendInlineKeyboard(botClient, msg),
            "/keyboard" => SendReplyKeyboard(botClient, msg),
            "/remove" => RemoveKeyboard(botClient, msg),
            "/request" => RequestContactAndLocation(botClient, msg),
            "/inline_mode" => StartInlineQuery(botClient, msg),
            "/poll" => SendPoll(botClient, msg),
            "/poll_anonymous" => SendAnonymousPoll(botClient, msg),
            "/throw" => FailingHandler(botClient, msg),
            _ => Usage(botClient, msg)
        });
        logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
    }

    async Task<Message> Usage(ITelegramBotClient botClient, Message msg)
    {
        const string usage = """
                <b><u>Bot menu</u></b>:
                /photo          - send a photo
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /request        - request location or contact
                /inline_mode    - send inline-mode results list
                /poll           - send a poll
                /poll_anonymous - send an anonymous poll
                /throw          - what happens if handler fails
            """;
        return await botClient.SendTextMessageAsync(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> SendPhoto(ITelegramBotClient botClient, Message msg)
    {
        await botClient.SendChatActionAsync(msg.Chat, ChatAction.UploadPhoto);
        await Task.Delay(2000); // simulate a long task
        await using var fileStream = new FileStream("Files/bot.gif", FileMode.Open, FileAccess.Read);
        return await botClient.SendPhotoAsync(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
    }

    // Send inline keyboard. You can process responses in OnCallbackQuery handler
    async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message msg)
    {
        var inlineMarkup = new InlineKeyboardMarkup()
            .AddNewRow("1.1", "1.2", "1.3")
            .AddNewRow()
                .AddButton("WithCallbackData", "CallbackData")
                .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
        return await botClient.SendTextMessageAsync(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
    }

    async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message msg)
    {
        var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddNewRow("1.1", "1.2", "1.3")
            .AddNewRow().AddButton("2.1").AddButton("2.2");
        return await botClient.SendTextMessageAsync(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);
    }

    async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message msg)
    {
        return await botClient.SendTextMessageAsync(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message msg)
    {
        var replyMarkup = new ReplyKeyboardMarkup(true)
            .AddButton(KeyboardButton.WithRequestLocation("Location"))
            .AddButton(KeyboardButton.WithRequestContact("Contact"));
        return await botClient.SendTextMessageAsync(msg.Chat, "Who or Where are you?", replyMarkup: replyMarkup);
    }

    async Task<Message> StartInlineQuery(ITelegramBotClient botClient, Message msg)
    {
        var button = InlineKeyboardButton.WithSwitchInlineQueryCurrentChat("Inline Mode");
        return await botClient.SendTextMessageAsync(msg.Chat, "Press the button to start Inline Query\n\n" +
            "(Make sure you enabled Inline Mode in @BotFather)", replyMarkup: new InlineKeyboardMarkup(button));
    }

    async Task<Message> SendPoll(ITelegramBotClient botClient, Message msg)
    {
        return await botClient.SendPollAsync(msg.Chat, "Question", PollOptions, isAnonymous: false);
    }

    async Task<Message> SendAnonymousPoll(ITelegramBotClient botClient, Message msg)
    {
        return await botClient.SendPollAsync(chatId: msg.Chat, "Question", PollOptions);
    }

    static Task<Message> FailingHandler(ITelegramBotClient botClient, Message msg)
    {
        throw new NotImplementedException("FailingHandler");
    }

    // Process Inline Keyboard callback data
    private async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Received {callbackQuery.Data}");
        await botClient.SendTextMessageAsync(callbackQuery.Message!.Chat, $"Received {callbackQuery.Data}");
    }

    #region Inline Mode

    private async Task OnInlineQuery(ITelegramBotClient botClient, InlineQuery inlineQuery)
    {
        logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results = [ // displayed result
            new InlineQueryResultArticle("1", "Telegram.Bot", new InputTextMessageContent("hello")),
            new InlineQueryResultArticle("2", "is the best", new InputTextMessageContent("world"))
        ];
        await botClient.AnswerInlineQueryAsync(inlineQuery.Id, results, cacheTime: 0, isPersonal: true);
    }

    private async Task OnChosenInlineResult(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
    {
        logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        await botClient.SendTextMessageAsync(chosenInlineResult.From.Id, $"You chose result with Id: {chosenInlineResult.ResultId}");
    }

    #endregion

    private Task OnPoll(ITelegramBotClient botClient, Poll poll)
    {
        logger.LogInformation("Received Poll info: {Question}", poll.Question);
        return Task.CompletedTask;
    }

    private async Task OnPollAnswer(ITelegramBotClient botClient, PollAnswer pollAnswer)
    {
        var answer = pollAnswer.OptionIds.FirstOrDefault();
        var selectedOption = PollOptions[answer];
        if (pollAnswer.User != null)
            await botClient.SendTextMessageAsync(pollAnswer.User.Id, $"You've chosen: {selectedOption.Text} in poll");
    }

    private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}

