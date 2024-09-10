using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;

namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// WebAPI扩展
/// </summary>
public static class WebAPIExtension
{
    /// <summary>
    /// 注册WebAPI
    /// </summary>
    /// <param name="services"></param>
    /// <param name="webHost"></param>
    public static void AddWebAPI(this IServiceCollection services, IWebHostBuilder webHost)
    {
        // 设置最大文件上传尺寸
        webHost.UseKestrel(options => options.Limits.MaxRequestBodySize = 1073741824);

        // 响应缓存
        services.AddResponseCaching();

        // 响应压缩
        services.AddResponseCompression(static o => o.EnableForHttps = true);

        // CORS
        services.AddCors(static options => options.AddDefaultPolicy(static p => p.AllowAnyOrigin()));

        // Swagger
        services.AddSwaggerEx();

        // 控制器
        services.AddControllers().AddJsonOptions(static o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // 注册服务
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        //获取客户端 IP
        services.Configure<ForwardedHeadersOptions>(o => {
            o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            o.KnownNetworks.Clear();
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

        bool isDevelopment = app.Environment.IsDevelopment();

        // 调试模式输出错误信息
        if (isDevelopment || app.Configuration.GetSection("Debug").Get<bool>())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        // Swagger
        if (isDevelopment || app.Configuration.GetSection("Swagger").Get<bool>())
        {
            app.UseSwaggerEx();
        }

        app.UseStatusCodePages();

        // 控制器
        app.MapControllers();
    }
}
