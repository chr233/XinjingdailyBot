using XhhControlSystem.SignalR.Hubs;

namespace XhhControlSystem.Server.Extensions;

/// <summary>
/// SignalR扩展
/// </summary>
public static class SignalRExtension
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// 添加SignalR服务
        /// </summary>
        /// <param name="services"></param>
        public void AddSignalREx()
        {
            services.AddSignalR();//.AddMessagePackProtocol();
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// 注册SignalR Hub
        /// </summary>
        /// <param name="app"></param>
        public void UseSignalRHub()
        {
            app.MapHub<PluginHub>("/plugin");
        }
    }
}