using XinjingdailyBot.Tasks;

namespace XinjingdailyBot.WebAPI.Extensions
{
    /// <summary>
    /// Telegram扩展
    /// </summary>
    public static class TaskExtension
    {
        /// <summary>
        /// 注册定时任务
        /// </summary>
        /// <param name="services"></param>
        public static void AddTasks(this IServiceCollection services)
        {
            services.AddHostedService<PollingService>();
            services.AddHostedService<ExpiredPostsTask>();
            services.AddHostedService<RejectChannelTask>();
            services.AddHostedService<PostAdvertiseTask>();
        }
    }
}
