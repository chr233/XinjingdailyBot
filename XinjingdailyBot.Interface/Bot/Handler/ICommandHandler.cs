using System.Diagnostics.CodeAnalysis;
using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    public interface ICommandHandler
    {
        string GetAvilabeCommands(Users dbUser);
        Task<bool> GetCommandsMenu();

        [RequiresUnreferencedCode("不兼容剪裁")]
        void InstallCommands();
        Task OnCommandReceived(Users dbUser, Message message);
        Task OnQueryCommandReceived(Users dbUser, CallbackQuery query);
    }
}
