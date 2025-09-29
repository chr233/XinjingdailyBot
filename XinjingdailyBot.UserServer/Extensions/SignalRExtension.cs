using XhhControlSystem.SignalR.Hubs;

namespace XhhControlSystem.Server.Extensions;

/// <summary>
/// SignalR扩展
/// </summary>
public static class SignalRExtension
{
    /// <summary>
    /// 添加SignalR服务
    /// </summary>
    /// <param name="services"></param>
    public static void AddSignalREx(this IServiceCollection services)
    {
        services.AddSignalR();//.AddMessagePackProtocol();
    }


    /// <summary>
    /// 注册SignalR Hub
    /// </summary>
    /// <param name="app"></param>
    public static void UseSignalRHub(this WebApplication app)
    {
        app.MapHub<PluginHub>("/plugin");
    }
}