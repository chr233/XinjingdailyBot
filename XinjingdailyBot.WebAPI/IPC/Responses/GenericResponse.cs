using System.Net;

namespace XinjingdailyBot.WebAPI.IPC.Responses;

/// <summary>
/// 通用响应
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record GenericResponse<T> : GenericResponse where T : notnull
{
    /// <summary>
    /// 结果
    /// </summary>
    public T? Result { get; private set; }
}

/// <summary>
/// 通用响应
/// </summary>
public record GenericResponse
{
    /// <summary>
    /// 响应码
    /// </summary>
    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; } = "OK";

    /// <summary>
    /// 成功
    /// </summary>
    public bool Success { get; set; }
}
