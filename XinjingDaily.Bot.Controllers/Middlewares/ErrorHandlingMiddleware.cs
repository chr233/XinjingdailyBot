using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using XinjingDaily.Bot.Controllers.Responses;
using XinjingDaily.Bot.WebAPI.IPC.Responses;

namespace XinjingDaily.Bot.Controllers.Middleware;

/// <summary>
/// 错误处理中间件
/// </summary>
/// <param name="_next"></param>
/// <param name="_logger"></param>
public class ErrorHandlingMiddleware(
    RequestDelegate _next,
    ILogger<ErrorHandlingMiddleware> _logger)
{
    /// <inheritdoc/>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "系统错误");

            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new GenericResponse(500, exception.Message);
        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
