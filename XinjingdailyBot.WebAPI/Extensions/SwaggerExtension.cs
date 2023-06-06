using Microsoft.OpenApi.Models;

namespace XinjingdailyBot.WebAPI.Extensions
{
    /// <summary>
    /// Swagger扩展
    /// </summary>
    public static class SwaggerExtension
    {
        /// <summary>
        /// 注册Swagger
        /// </summary>
        /// <param name="services"></param>
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo {
                    Version = "v1",
                    Title = "XinjingdailyBot API",
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
            });
        }
    }
}
