using System.Net;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot
{
    internal class Program
    {
        /// <summary>
        /// 启动入口
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            await ConfigHelper.LoadConfig();

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(10, 10);

            TelegramBotClient bot;

            if (!string.IsNullOrEmpty(BotConfig.Proxy))
            {
                WebProxy proxy = new()
                {
                    Address = new Uri(BotConfig.Proxy),
                };

                HttpClientHandler handler = new()
                {
                    Proxy = proxy
                };

                HttpClient httpClient = new(handler);

                bot = new TelegramBotClient(BotConfig.BotToken, httpClient);
            }
            else
            {
                bot = new TelegramBotClient(BotConfig.BotToken);
            }

            BotID = bot.BotId ?? 0;

            Logger.Info(BotName);

            using var cts = new CancellationTokenSource();

            try
            {
                Logger.Info("读取频道信息");

                await ChannelHelper.VerifyChannelConfig(bot);

                Logger.Info("初始化数据库");

                await DataBaseHelper.Init();

                DB.Ado.CommandTimeOut = 30;
                if (!DB.Ado.IsValidConnection())
                {
                    Logger.Error("数据库连接失败");
                    Logger.Info("按任意键退出…");
                    Console.ReadKey();
                    return;
                }

                bot.StartReceiving(
                    updateHandler: Handlers.Dispatcher.HandleUpdateAsync,
                    errorHandler: Handlers.Dispatcher.HandleErrorAsync,
                    receiverOptions: new ReceiverOptions()
                    {
                        AllowedUpdates = Array.Empty<UpdateType>()
                    },
                    cancellationToken: cts.Token
                );

                Logger.Info("Bot开始运行, 回车键退出进程");
                Console.ReadLine();
                Logger.Info("Bot即将退出");
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Logger.Info("运行出错, 按任意键退出");
                Console.ReadKey();
            }
            finally
            {
                ConfigHelper.SaveConfig();
            }
        }
    }
}