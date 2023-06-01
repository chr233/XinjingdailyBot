using Telegram.Bot.Types;
using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Bot.Handler
{
    /// <summary>
    /// 申请加群处理器
    /// </summary>
    public interface IJoinRequestHandler
    {
        /// <summary>
        /// 投稿超过设定自动同意加群请求
        /// </summary>
        public static readonly int AutoApproveLimit = 5;
        /// <summary>
        /// 收到加群请求处理器
        /// </summary>
        /// <param name="dbUser"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task OnJoinRequestReceived(Users dbUser, ChatJoinRequest request);
    }
}
