using Newtonsoft.Json;
using XinjingdailyBot.Infrastructure.Constant;
using XinjingdailyBot.Infrastructure.CustomException;

namespace XinjingdailyBot.Infrastructure.Model
{
    public class ApiResult
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        /// <summary>
        /// 如果data值为null，则忽略序列化将不会返回data字段
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }

        /// <summary>
        /// 初始化一个新创建的APIResult对象，使其表示一个空消息
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的 属性“Msg”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        public ApiResult()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的 属性“Msg”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        {
        }

        /// <summary>
        /// 初始化一个新创建的 ApiResult 对象
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        public ApiResult(int code, string msg)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        {
            Code = code;
            Msg = msg;
        }

        /// <summary>
        /// 初始化一个新创建的 ApiResult 对象
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        public ApiResult(int code, string msg, object data)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的 属性“Data”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        {
            Code = code;
            Msg = msg;
            if (data != null)
            {
                Data = data;
            }
        }

        /// <summary>
        /// 返回成功消息
        /// </summary>
        /// <returns></returns>
        public ApiResult Success()
        {
            Code = (int)ResultCode.SUCCESS;
            Msg = "success";
            return this;
        }

        /// <summary>
        /// 返回成功消息
        /// </summary>
        /// <param name="data">数据对象</param>
        /// <returns>成功消息</returns>
        public static ApiResult Success(object data) { return new ApiResult(HttpStatus.SUCCESS, "success", data); }

        /// <summary>
        /// 返回成功消息
        /// </summary>
        /// <param name="msg">返回内容</param>
        /// <returns>成功消息</returns>
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
        public static ApiResult Success(string msg) { return new ApiResult(HttpStatus.SUCCESS, msg, null); }
#pragma warning restore CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

        /// <summary>
        /// 返回成功消息
        /// </summary>
        /// <param name="msg">返回内容</param>
        /// <param name="data">数据对象</param>
        /// <returns>成功消息</returns>
        public static ApiResult Success(string msg, object data) { return new ApiResult(HttpStatus.SUCCESS, msg, data); }

        /// <summary>
        /// 访问被拒
        /// </summary>
        /// <param name="apiResult"></param>
        /// <returns></returns>
        public ApiResult On401()
        {
            Code = (int)ResultCode.DENY;
            Msg = "access denyed";
            return this;
        }
        public ApiResult Error(ResultCode resultCode, string msg = "")
        {
            Code = (int)resultCode;
            Msg = msg;
            return this;
        }

        /// <summary>
        /// 返回失败消息
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult Error(int code, string msg) { return new ApiResult(code, msg); }

        /// <summary>
        /// 返回失败消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static ApiResult Error(string msg) { return new ApiResult((int)ResultCode.CUSTOM_ERROR, msg); }

        public override string ToString()
        {
            return $"msg={Msg},data={Data}";
        }
    }

    public class ApiResult<T> : ApiResult
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的 属性“Result”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
        public T Result { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的 属性“Result”必须包含非 null 值。请考虑将 属性 声明为可以为 null。
    }
}
