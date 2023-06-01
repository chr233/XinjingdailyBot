using SqlSugar;
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

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(typeof(IInlineQueryHandler), LifeTime.Singleton)]
    internal class InlineQueryHandler : IInlineQueryHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IAttachmentService _attachmentService;
        private readonly IPostService _postService;
        private readonly IMarkupHelperService _markupHelperService;

        public InlineQueryHandler(
            ITelegramBotClient botClient,
            IAttachmentService attachmentService,
            IPostService postService,
            IMarkupHelperService markupHelperService)
        {
            _botClient = botClient;
            _attachmentService = attachmentService;
            _postService = postService;
            _markupHelperService = markupHelperService;
        }

        public async Task OnInlineQueryReceived(Users dbUser, InlineQuery query)
        {
            if (dbUser.Right.HasFlag(EUserRights.AdminCmd))
            {
                List<InlineQueryResult> results = new();

                for (int i = 0; i < 10; i++)
                {
                    var randomPost = await _postService.Queryable()
                        .Where(x => x.Status == EPostStatus.Accepted && x.PostType == MessageType.Photo)
                        .OrderBy(x => SqlFunc.GetRandom()).Take(1).FirstAsync();

                    if (randomPost == null)
                    {
                        break;
                    }

                    var postAttachment = await _attachmentService.Queryable().Where(x => x.PostID == randomPost.Id).FirstAsync();
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
                InlineQueryResult[] results = {
                    new InlineQueryResultArticle("1", "该功能暂时仅对管理员开放", new InputTextMessageContent("To be continued"))
                };

                await _botClient.AnswerInlineQueryAsync(inlineQueryId: query.Id, results: results, cacheTime: 10, isPersonal: true);
            }
        }
    }
}
