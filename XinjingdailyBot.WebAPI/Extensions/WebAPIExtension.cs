using XinjingdailyBot.WebAPI.IPC.Middlewares;

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
        services.AddResponseCompression(static options => options.EnableForHttps = true);

        // CORS
        services.AddCors(static options => options.AddDefaultPolicy(static policy => policy.AllowAnyOrigin()));

        // Swagger
        services.AddSwaggerEx();

        // 控制器
        services.AddControllers();

        // 注册服务
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

        // 身份验证中间件, 仅在 /Api 路径下启用
        app.UseWhen(static context =>
            context.Request.Path.StartsWithSegments("/Api", StringComparison.OrdinalIgnoreCase),
            static appBuilder => appBuilder.UseMiddleware<ApiAuthenticationMiddleware>()
        );

        // 控制器
        app.MapControllers();
    }
}
