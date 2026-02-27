using System.Text.Json.Serialization;

namespace XinjingDaily.Bot.Controllers.Responses;

/// <summary>
/// 通用响应
/// </summary>
/// <typeparam name="T"></typeparam>
public record GenericResponse<T> : GenericResponse where T : notnull
{
    /// <summary>
    /// 结果
    /// </summary>
    [JsonPropertyName("result")]
    public T? Result { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public GenericResponse(int code, T? result) : base(code)
    {
        Result = result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="result"></param>
    public GenericResponse(int code, string? message, T? result) : base(code, message)
    {
        Result = result;
    }
}

/// <summary>
/// 通用响应
/// </summary>
public record GenericResponse
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly GenericResponse ParamErrorResponse = new(422, "参数错误");
    public static readonly GenericResponse InternalErrorResponse = new(500, "内部错误");
    /// <summary>
    /// 
    /// </summary>
    public static readonly GenericResponse OkResponse = new(200, "ok");
    /// <summary>
    /// 
    /// </summary>
    public static readonly GenericResponse DeleteSuccessResponse = new(200, "删除成功");

    public static readonly GenericResponse AuthorizeParamMissingResponse = new(401, "缺少 Header 参数 Authorization");
    public static readonly GenericResponse AuthorizeFailedResponse = new(401, "Authorization 失败");

    /// <summary>
    /// 响应码
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("msg")]
    public string? Message { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    public bool Success => Code == 200;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    public GenericResponse(int code)
    {
        Code = code;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    public GenericResponse(int code, string? message)
    {
        Code = code;
        Message = message;
    }
}
