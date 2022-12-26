using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot.Common
{
    public interface IChannelService
    {
        Chat ReviewGroup { get; }
        Chat CommentGroup { get; }
        Chat SubGroup { get; }
        Chat AcceptChannel { get; }
        Chat RejectChannel { get; }
        User BotUser { get; }

        Task InitChannelInfo();
    }
}
