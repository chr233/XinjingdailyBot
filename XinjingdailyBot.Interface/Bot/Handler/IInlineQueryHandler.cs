using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler;

/// <summary>
/// InlineQuery查询处理器
/// </summary>
public interface IInlineQueryHandler
{
    /// <summary>
    /// 收到InlineQuery查询请求
    /// </summary>
    /// <param name="dbUser"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    Task OnInlineQueryReceived(Users dbUser, InlineQuery query);
}
