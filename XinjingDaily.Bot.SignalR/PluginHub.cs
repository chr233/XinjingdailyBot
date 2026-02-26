using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace XhhControlSystem.SignalR.Hubs;

/// <summary>
/// Plugin Hub
/// </summary>
/// <param name="_logger"></param>
/// <param name="_sessionService"></param>
public class PluginHub(
    ILogger<PluginHub> _logger) : Hub
{
    #region 客户端校验
    /// <inheritdoc/>
    //public override async Task OnConnectedAsync()
    //{
    //    var httpCtx = Context.GetHttpContext();
    //    if (httpCtx != null)
    //    {
    //        var token = httpCtx.Request.Headers.Authorization;
    //        var info = await _sessionService.RegisterPlugin(Context.ConnectionId, token).ConfigureAwait(false);

    //        if (info != null)
    //        {
    //            _logger.LogInformation("Plugin 连接成功 {info}", info);

    //            Context.Items["info"] = info;

    //            await base.OnConnectedAsync().ConfigureAwait(false);
    //            return;
    //        }
    //    }

    //    _logger.LogWarning("Plugin 被拒绝连接 {id}", Context.ConnectionId);
    //    Context.Abort();
    //}

    ///// <inheritdoc/>
    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    var info = GetPluginInfo();
    //    if (info != null)
    //    {
    //        await _sessionService.RemovePlugin(info).ConfigureAwait(false);
    //    }

    //    if (exception == null)
    //    {
    //        _logger.LogWarning("Plugin 断开连接 {info}", info);
    //    }
    //    else
    //    {
    //        _logger.LogError(exception, "Plugin 断开连接 {info}", info);
    //    }

    //    await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    //}
    #endregion

    ///// <summary>
    ///// Echo
    ///// </summary>
    ///// <param name="message"></param>
    ///// <returns></returns>
    //public Task<string> Echo(string message)
    //{
    //    var info = GetPluginInfo();
    //    _logger.LogInformation("Plugin {info} Echo: {msg}", info, message);
    //    return Task.FromResult(message);
    //}
}