using Telegram.Bot;
using Telegram.Bot.Types;
using XinjingdailyBot.Enums;
using XinjingdailyBot.Helpers;
using XinjingdailyBot.Models;

namespace XinjingdailyBot.Handlers.Messages
{
    internal class CommandHandler
    {
        internal static async Task<string?> ExecCommand(ITelegramBotClient botClient, Users dbUser, Message message)
        {
            string input = message.Text![1..];

            bool normal = dbUser.Right.HasFlag(UserRights.NormalCmd);
            bool admin = dbUser.Right.HasFlag(UserRights.AdminCmd);
            bool super = dbUser.Right.HasFlag(UserRights.SuperCmd);

            string[] args = input.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            if (message.ReplyToMessage != null)
            {
                Users? targetUser = await FetchUserHelper.FetchDbUser(message.ReplyToMessage);

                switch (args.Length)
                {
                    case 0:
                        return null;
                    case 1://不带参数
                        switch (args[0].ToUpperInvariant())
                        {
                            case "HELP":
                                return "HELP";

                            case "BAN" when (admin || super) && targetUser != null:
                                return await Commands.AdminCmd.BanUser(dbUser, targetUser);


                        }

                        return null;
                        break;
                    default://带参数
                        switch (args[0].ToUpperInvariant())
                        {

                        }
                        return null;
                        break;
                }

            }
            else
            {
                switch (args.Length)
                {
                    case 0:
                        return null;
                    case 1://不带参数
                        switch (args[0].ToUpperInvariant())
                        {
                            case "HELP":
                                return "HELP";


                        }

                        return null;
                        break;
                    default://带参数
                        switch (args[0].ToUpperInvariant())
                        {

                        }
                        return null;
                        break;
                }
            }

            await botClient.AutoReplyAsync(args.Length.ToString(), message);

            return null;
        }
    }
}
