using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Tasks;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot
{
    internal static class Program
    {
        /// <summary>
        /// 启动入口
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [STAThread]
        public static async Task Main()
        {
            var exitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                exitEvent.Set();
            };

            try
            {
                Logger.Info("--系统初始化--");

                await ConfigHelper.LoadConfig();

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

                var botClient = new TelegramBotClient(BotConfig.BotToken, httpClient);

                using var cts = new CancellationTokenSource();

                Logger.Info("--读取基础信息--");
                await ChannelHelper.VerifyChannelConfig(botClient);

                Logger.Info("--初始化数据库--");
                await DataBaseHelper.Init();

                DB.Ado.CommandTimeOut = 30;
                if (!DB.Ado.IsValidConnection())
                {
                    Logger.Error("--数据库连接失败--");
                    throw new Exception("数据库连接失败");
                }

                Logger.Info("数据库初始化完成");

                botClient.StartReceiving(
                    updateHandler: Handlers.UpdateDispatcher.HandleUpdateAsync,
                    pollingErrorHandler: Handlers.UpdateDispatcher.HandleErrorAsync,
                    receiverOptions: new ReceiverOptions()
                    {
                        AllowedUpdates = Array.Empty<UpdateType>()
                    },
                    cancellationToken: cts.Token
                );

                //TaskHelper.InitTasks(botClient);

                Logger.Info("--开始运行, Ctrl+C 结束运行--");
                exitEvent.WaitOne();
                Logger.Info("--运行结束, 即将退出--");
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
