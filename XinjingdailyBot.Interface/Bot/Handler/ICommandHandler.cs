using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface ICommandHandler
    {
        void InstallCommands();
        Task OnCommandReceived(Users dbUser, Message message);
        Task OnQueryCommandReceived(Users dbUser, CallbackQuery query);
    }
}
