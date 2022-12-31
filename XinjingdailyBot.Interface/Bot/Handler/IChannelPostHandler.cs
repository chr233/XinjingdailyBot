using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface IChannelPostHandler
    {
        Task OnMediaChannelPostReceived(Users dbUser, Message message);
        Task OnMediaGroupChannelPostReceived(Users dbUser, Message message);
        Task OnTextChannelPostReceived(Users dbUser, Message message);
    }
}
