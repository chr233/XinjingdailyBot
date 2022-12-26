using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface ICommandHandler
    {
        Task InitCommands();
        Task OnCommandReceived(Users dbUser, Message message);
    }
}
