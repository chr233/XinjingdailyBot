using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using XinjingdailyBot.Infrastructure;

namespace SteamFamily.API.Extensions;

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
            o.Title = BuildInfo.AppName;
            o.Description = BuildInfo.Copyright;

            o.AddSecurity("Token", new NSwag.OpenApiSecurityScheme {
                Name = "aaa",
                Description = "123"
            });

        });
    }

    /// <summary>
    /// 注册Swagger
    /// </summary>
    /// <param name="app"></param>
    public static void UseSwaggerEx(this WebApplication app)
    {
        var optionsAccessor = app.Services.GetService<IOptions<OptionsSetting>>();
        if (optionsAccessor == null || !optionsAccessor.Value.System.Swagger)
        {
            return;
        }

        app.UseOpenApi(static o => {
            o.Path = "/openapi/{documentName}.json";
        });
        app.MapScalarApiReference("/swagger");
    }
}
