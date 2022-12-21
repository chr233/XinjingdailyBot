using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Extensions;
using XinjingdailyBot.Interface.Bot.Dispatcher;
using XinjingdailyBot.Interface.Data;
using XinjingdailyBot.Interface.Helper;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Dispatcher
{
    [AppService(ServiceType = typeof(IQueryDispatcherService), ServiceLifetime = LifeTime.Scoped)]
    public class QueryDispatcherService : IQueryDispatcherService
    {
        private readonly ILogger<QueryDispatcherService> _logger;
        private readonly ITextHelperService _userService;
        private readonly ITelegramBotClient _botClient;
        private readonly IPostService _postService;

        private static readonly TimeSpan IgnoreQueryOlderThan = TimeSpan.FromSeconds(30);

        public QueryDispatcherService(
            ILogger<QueryDispatcherService> logger,
            ITextHelperService userService,
            ITelegramBotClient botClient,
            IPostService postService)
        {
            _logger = logger;
            _userService = userService;
            _botClient = botClient;
            _postService = postService;
        }

        /// <summary>
        /// 忽略旧的CallbackQuery
        /// </summary>

        /// <summary>
        /// 处理CallbackQuery
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="dbUser"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        public async Task OnCallbackQueryReceived(Users dbUser, CallbackQuery callbackQuery)
        {
            //检查是否封禁
            if (dbUser.IsBan)
            {
                await _botClient.AutoReplyAsync("无权访问", callbackQuery);
                return;
            }

            Message? message = callbackQuery.Message;
            if (message == null)
            {
                await _botClient.AutoReplyAsync("消息不存在", callbackQuery);
                return;
            }

            string? data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data))
            {
                await _botClient.RemoveMessageReplyMarkupAsync(message);
                return;
            }

            _logger.LogCallbackQuery(callbackQuery);

            //忽略过旧的Query
            if (DateTime.Now - message.Date > IgnoreQueryOlderThan)
            {
                //return;
            }

            string[] args = data.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
            if (!args.Any()) { return; }

            string cmd = args.First();
            args = args[1..];

            switch (cmd)
            {
                //投稿确认
                case "post":
                    await PostHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                //审核相关
                case "review":
                case "reject":
                    await ReviewHandler.HandleQuery(botClient, dbUser, callbackQuery);
                    break;

                //命令回调
                case "cmd":
                    await CommandHandler.HandleQuery(botClient, dbUser, callbackQuery, args);
                    break;

                //取消操作
                case "cancel":
                    await _botClient.AutoReplyAsync("操作已取消", callbackQuery);
                    await _botClient.EditMessageTextAsync(message, "操作已取消", replyMarkup: null);
                    break;

                //无动作
                case "none":
                    await _botClient.AutoReplyAsync("无", callbackQuery);
                    break;

                default:
                    break;
            }
        }
    }
}
