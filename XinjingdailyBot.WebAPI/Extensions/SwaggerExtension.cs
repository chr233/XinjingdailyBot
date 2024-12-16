using Microsoft.OpenApi.Models;
using XinjingdailyBot.Controllers.Authorization;
using XinjingdailyBot.WebAPI.IPC.Middlewares;

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
        services.AddSwaggerGen(static o => {
            o.SwaggerDoc(
                "v1", new OpenApiInfo {
                    Version = "v1",
                    Title = "XinjingdailyBot WebAPI",
                    Description = "",
                    Contact = new OpenApiContact {
                        Name = "心惊报",
                        Url = new Uri("https://t.me/xinjingdaily")
                    },
                    License = new OpenApiLicense {
                        Name = "AGPL 3.0",
                        Url = new Uri("https://github.com/chr233/XinjingdailyBot/blob/main/LICENSE.txt")
                    },
                });

            var scheme = new OpenApiSecurityScheme {
                Type = SecuritySchemeType.ApiKey,
                Description = "用户登录 Token, 使用命令 /token 获取",
                Name = VerifyAttribute.HeaderName,
                In = ParameterLocation.Header,
            };

            o.AddSecurityDefinition(VerifyAttribute.FieldName, scheme);

            o.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Id = VerifyAttribute.FieldName,
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            );

            o.CustomSchemaIds(static type => type.ToString());

            o.EnableAnnotations(true, true);

            o.SchemaFilter<EnumSchemaFilter>();

            // 文档注释
            var xmlFiles = Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var file in xmlFiles)
            {
                o.IncludeXmlComments(file);
            }
        });
    }

    /// <summary>
    /// 注册Swagger
    /// </summary>
    /// <param name="app"></param>
    public static void UseSwaggerEx(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(static o => {
            o.DisplayRequestDuration();
            o.EnableDeepLinking();
            o.ShowExtensions();
            o.EnableTryItOutByDefault();
            o.EnablePersistAuthorization();
            o.EnableFilter();
            o.EnableValidator();
        });
    }
}
