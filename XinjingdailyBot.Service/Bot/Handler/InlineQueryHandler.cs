using Microsoft.Extensions.Logging;
using SqlSugar;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Interface.Bot.Common;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    [AppService(typeof(IInlineQueryHandler), LifeTime.Singleton)]
    public class InlineQueryHandler : IInlineQueryHandler
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
            if (dbUser.Right.HasFlag(UserRights.AdminCmd))
            {
                Dictionary<string, BuildInTags> tags = new()
                {
                    { "随机稿件", BuildInTags.None },
                    { "随机 #NSFW 稿件", BuildInTags.NSFW },
                    { "随机 #我有一个朋友 稿件", BuildInTags.Friend },
                    { "随机 #晚安 稿件", BuildInTags.WanAn },
                    { "随机 #AI怪图 稿件", BuildInTags.AIGraph },
                };

                List<InlineQueryResult> results = new();

                foreach (var (id, tag) in tags)
                {
                    var randomPost = await _postService.Queryable()
                        .WhereIF(tag == BuildInTags.None, x => x.Status == PostStatus.Accepted && x.PostType == MessageType.Photo)
                        .WhereIF(tag != BuildInTags.None, x => x.Status == PostStatus.Accepted && x.PostType == MessageType.Photo && ((byte)x.Tags & (byte)tag) > 0)
                        .OrderBy(x => SqlFunc.GetRandom()).Take(1).FirstAsync();

                    if (randomPost != null)
                    {
                        var postAttachment = await _attachmentService.Queryable().Where(x => x.PostID == randomPost.Id).FirstAsync();
                        var keyboard = _markupHelperService.LinkToOriginPostKeyboard(randomPost);
                        results.Add(new InlineQueryResultCachedPhoto(id, postAttachment.FileID)
                        {
                            Title = id,
                            Description = id,
                            Caption = randomPost.Text,
                            ParseMode = ParseMode.Html,
                            ReplyMarkup = keyboard
                        });
                    }
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
                    new InlineQueryResultArticle(
                        id: "1",
                        title: "该功能暂时仅对管理员开放",
                        inputMessageContent: new InputTextMessageContent("To be continued"))
                };

                await _botClient.AnswerInlineQueryAsync(inlineQueryId: query.Id, results: results, cacheTime: 10, isPersonal: true);
            }
        }
    }
}
