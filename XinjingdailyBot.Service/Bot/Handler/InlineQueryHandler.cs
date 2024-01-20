using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler;

/// <inheritdoc cref="IInlineQueryHandler"/>
[AppService(typeof(IInlineQueryHandler), LifeTime.Singleton)]
public sealed class InlineQueryHandler(
        ITelegramBotClient _botClient,
        IAttachmentService _attachmentService,
        IPostService _postService,
        IMarkupHelperService _markupHelperService) : IInlineQueryHandler
{
    /// <inheritdoc/>
    public async Task OnInlineQueryReceived(Users dbUser, InlineQuery query)
    {
        if (dbUser.Right.HasFlag(EUserRights.AdminCmd))
        {
            var results = new List<InlineQueryResult>();

            for (int i = 0; i < 10; i++)
            {
                var randomPost = await _postService.GetRandomPost();

                if (randomPost == null)
                {
                    break;
                }

                var postAttachment = await _attachmentService.FetchAttachmentByPostId(randomPost.Id);
                var keyboard = _markupHelperService.LinkToOriginPostKeyboard(randomPost);
                results.Add(new InlineQueryResultCachedPhoto(i.ToString(), postAttachment.FileID) {
                    Title = randomPost.Text,
                    Description = randomPost.Text,
                    Caption = randomPost.Text,
                    ParseMode = ParseMode.Html,
                    ReplyMarkup = keyboard
                });
            }

            if (results.Count == 0)
            {
                results.Add(new InlineQueryResultArticle("1", "没有可用稿件", new InputTextMessageContent("没有可用稿件")));
            }

            await _botClient.AnswerInlineQueryAsync(
                inlineQueryId: query.Id,
                results: results,
                cacheTime: 10,
                isPersonal: true);
        }
        else
        {
            InlineQueryResult[] results = [
                new InlineQueryResultArticle("1", "该功能暂时仅对管理员开放", new InputTextMessageContent("To be continued"))
            ];

            await _botClient.AnswerInlineQueryAsync(inlineQueryId: query.Id, results: results, cacheTime: 10, isPersonal: true);
        }
    }
}
