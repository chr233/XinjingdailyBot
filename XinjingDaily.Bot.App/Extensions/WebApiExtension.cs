using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using XinjingDaily.Bot.Controllers.Middleware;
using XinjingDaily.Bot.Infrastructure;

namespace XinjingDaily.Bot.WebAPI.Extensions;

/// <summary>
/// WebAPI扩展
/// </summary>
public static class WebApiExtension
{
    /// <summary>
    /// 注册WebAPI
    /// </summary>
    /// <param name="services"></param>
    /// <param name="webHost"></param>
    public static void AddWebAPI(this IServiceCollection services, IWebHostBuilder webHost)
    {
        // 响应缓存
        services.AddResponseCaching();

        // 响应压缩
        services.AddResponseCompression(static o => o.EnableForHttps = true);

        // CORS
        services.AddCors(static options => options.AddDefaultPolicy(static p => p.AllowAnyOrigin()));

        // Swagger
        services.AddSwaggerEx();

        // 控制器
        var mvcBuilder = services.AddControllers();

        // 设置Json序列化行为
        mvcBuilder.AddJsonOptions(static o => {
            var option = o.JsonSerializerOptions;

            option.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            option.WriteIndented = false;
            option.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            option.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            option.PropertyNameCaseInsensitive = false;
            option.ReadCommentHandling = JsonCommentHandling.Skip;
        });

        // 注册服务
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        //获取客户端 IP
        services.Configure<ForwardedHeadersOptions>(o => {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
#if NET10_0_OR_GREATER
            o.KnownIPNetworks.Clear();
#else 
            o.KnownNetworks.Clear();
#endif
            o.KnownProxies.Clear();
        });
    }

    /// <summary>
    /// 注册WebAPI
    /// </summary>
    /// <param name="app"></param>
    public static void UseWebAPI(this WebApplication app)
    {
        // 响应缓存
        app.UseResponseCaching();

        // 响应压缩
        app.UseResponseCompression();

        // 支持CORS
        app.UseCors();

        var config = app.Services.GetRequiredService<IOptions<AppSettings>>().Value.System;

        // 调试模式输出错误信息
        if (config.Debug)
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        // Swagger
        if (config.Swagger)
        {
            app.UseSwaggerEx();
        }

        app.UseStatusCodePages();

        //app.UseAuthentication();
        //app.UseAuthorization();

        // 添加自定义中间件
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // 控制器
        app.MapControllers();
    }
}
