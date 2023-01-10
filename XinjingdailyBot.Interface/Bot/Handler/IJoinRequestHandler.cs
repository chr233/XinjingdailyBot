using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IJoinRequestHandler
    {
        Task OnJoinRequestReceived(Users dbUser, ChatJoinRequest request);
    }
}
