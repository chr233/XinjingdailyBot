using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Infrastructure.Attribute;
using XinjingdailyBot.Interface.Bot.Handler;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler;

[AppService(typeof(IJoinRequestHandler), LifeTime.Scoped)]
internal class JoinRequestHandler : IJoinRequestHandler
{
    private readonly ILogger<JoinRequestHandler> _logger;
    private readonly ITelegramBotClient _botClient;

    public JoinRequestHandler(
        ILogger<JoinRequestHandler> logger,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task OnJoinRequestReceived(Users dbUser, ChatJoinRequest request)
    {
        if (dbUser.AcceptCount >= IJoinRequestHandler.AutoApproveLimit)
        {
            try
            {
                await _botClient.ApproveChatJoinRequest(request.Chat.Id, dbUser.UserID);

                _logger.LogInformation("自动通过了 {user} 的加群请求", dbUser);

                if (dbUser.PrivateChatID != -1)
                {
                    await _botClient.SendTextMessageAsync(dbUser.PrivateChatID, $"欢迎加入 {request.Chat.Title}, 如果有其他验证记得手动完成");
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("自动同意加群请求失败, 机器人可能没有权限");
            }
        }
    }
}
