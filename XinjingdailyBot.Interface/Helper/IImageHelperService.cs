using Telegram.Bot.Types;

namespace XinjingdailyBot.Interface.Helper;

/// <summary>
/// 图片处理服务
/// </summary>
public interface IImageHelperService
{
    Task<string?> FuzzyImageCheck(Message message);
}
