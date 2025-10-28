using XinjingdailyBot.Infrastructure;

namespace XinjingdailyBot.WebAPI.Extensions;

/// <summary>
/// Swagger扩展
/// </summary>
public static class SwaggerExtension
{
    /// <summary>
    /// 注册Swagger
    /// </summary>
    /// <param name="services"></param>
    public static void AddSwaggerEx(this IServiceCollection services)
    {
        //Swagger
        services.AddEndpointsApiExplorer();

        services.AddOpenApiDocument(static o => {
            o.Title = "XinjingdailyBot API";
            o.Description = BuildInfo.Copyright;
        });
    }

    /// <summary>
    /// 注册Swagger
    /// </summary>
    /// <param name="app"></param>
    public static void UseSwaggerEx(this WebApplication app)
    {
        app.MapOpenApi();

        app.UseOpenApi();
        app.UseSwaggerUi(static o => {
            o.DocExpansion = "list";
            o.EnableTryItOut = true;
        });
        app.UseReDoc();
    }
}
