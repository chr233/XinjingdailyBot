using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Interface.Data
{
    /// <summary>
    /// 曾用名历史记录服务
    /// </summary>
    public interface INameHistoryService
    {
        /// <summary>
        /// 创建用户曾用名历史记录
        /// </summary>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        Task CreateNameHistory(Users dbUser);
    }
}
