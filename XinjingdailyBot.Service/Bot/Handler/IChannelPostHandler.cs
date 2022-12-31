using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Bot.Handler
{
    public interface IChannelPostHandler
    {
        Task OnMediaChannelPost(Users dbUser, Message message);
        Task OnMediaGroupChannelPost(Users dbUser, Message message);
        Task OnTextChannelPost(Users dbUser, Message message);
    }
}