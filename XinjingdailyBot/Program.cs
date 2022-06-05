using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using XinjingdailyBot.Helpers;
using static XinjingdailyBot.Utils;

namespace XinjingdailyBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await ConfigHelper.LoadConfig();

            var bot = new TelegramBotClient(BotConfig.BotToken);

            using var cts = new CancellationTokenSource();

            try
            {
                Logger.Info("初始化数据库");

                DataBaseHelper.Init();

                DB.Ado.CommandTimeOut = 30;
                if (!DB.Ado.IsValidConnection())
                {
                    Logger.Error("数据库连接失败");
                    return;
                }

                bot.StartReceiving(updateHandler: Handlers.Dispatcher.HandleUpdateAsync,
                                   errorHandler: Handlers.Dispatcher.HandleErrorAsync,
                                   receiverOptions: new ReceiverOptions()
                                   {
                                       AllowedUpdates = Array.Empty<UpdateType>()
                                   },
                                   cancellationToken: cts.Token);

                Logger.Info("Bot开始运行, 回车键退出进程");

                Console.ReadLine();

                Logger.Info("Bot即将退出");

                cts.Cancel();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                ConfigHelper.SaveConfig();
            }
        }
    }
}