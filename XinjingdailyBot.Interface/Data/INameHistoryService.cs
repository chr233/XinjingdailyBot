using XinjingdailyBot.Model.Models;

namespace XinjingdailyBot.Service.Data
{
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
