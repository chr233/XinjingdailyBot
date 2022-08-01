using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
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
        [STAThread]
        static async Task Main()
        {
            await ConfigHelper.LoadConfig();

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(10, 10);

            //设置代理
            HttpClient? httpClient = null;
            if (!string.IsNullOrEmpty(BotConfig.Proxy))
            {
                httpClient = new(
                    new HttpClientHandler()
                    {
                        Proxy = new WebProxy() { Address = new Uri(BotConfig.Proxy) },
                        UseProxy = true,
                    }
                );
            }

            TelegramBotClient bot = new TelegramBotClient(BotConfig.BotToken, httpClient);

            using var cts = new CancellationTokenSource();
            try
            {
                Logger.Info("--读取基础信息--");
                await ChannelHelper.VerifyChannelConfig(bot);

                Logger.Info("--初始化数据库--");
                await DataBaseHelper.Init();

                DB.Ado.CommandTimeOut = 30;
                if (!DB.Ado.IsValidConnection())
                {
                    Logger.Error("--数据库连接失败--");
                    throw new Exception("数据库连接失败");
                }

                bot.StartReceiving(
                    updateHandler: Handlers.UpdateDispatcher.HandleUpdateAsync,
                    pollingErrorHandler: Handlers.UpdateDispatcher.HandleErrorAsync,
                    receiverOptions: new ReceiverOptions()
                    {
                        AllowedUpdates = Array.Empty<UpdateType>()
                    },
                    cancellationToken: cts.Token
                );

                Logger.Info("--开始运行, 回车键退出进程--");
                Console.ReadLine();
                Logger.Info("--即将退出--");
                cts.Cancel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Logger.Info("--运行出错, 按任意键退出--");
                Console.ReadKey();
            }
            finally
            {
                ConfigHelper.SaveConfig();
            }
        }
    }
}
