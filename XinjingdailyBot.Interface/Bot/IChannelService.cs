using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Bot;

/// <summary>
/// A marker interface for Update Receiver service
/// </summary>
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
