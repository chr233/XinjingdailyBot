using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Infrastructure.Enums;
using XinjingdailyBot.Infrastructure.Extensions;

namespace XinjingdailyBot.Command
{
    [AppService(ServiceLifetime = LifeTime.Scoped)]
    public class SuperCommand
    {
        private readonly ILogger<SuperCommand> _logger;
        private readonly ITelegramBotClient _botClient;

        public SuperCommand(
            ILogger<SuperCommand> logger,
            ITelegramBotClient botClient)
        {
            _logger = logger;
            _botClient = botClient;
        }

        /// <summary>
        /// 重启机器人
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [TextCmd("RESTART", UserRights.SuperCmd, Description = "重启机器人")]
        public async Task ResponseRestart(Message message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    Process.Start(Environment.ProcessPath!);
                }
                catch (Exception ex)
                {
                    _logger.LogError("遇到错误", ex);
                }

                await Task.Delay(2000);

                Environment.Exit(0);
            });

            var text = "机器人即将重启";
            await _botClient.SendCommandReply(text, message);
        }
    }
}
