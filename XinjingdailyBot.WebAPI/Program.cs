
using System.Reflection;
using NLog.Extensions.Logging;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.WebAPI.Extensions;

namespace XinjingdailyBot.WebAPI
{
    public static class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
                _logger.Info("欢迎使用 XinjingdailyBot Version: {0}", version);
            }

            var builder = WebApplication.CreateBuilder(args);

            //配置类支持
            builder.Services.AddOptions().Configure<OptionsSetting>(builder.Configuration);

            //NLog
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddNLog();
            });

            //SqlSugar
            builder.Services.AddSqlSugar(builder.Configuration);

            //添加服务
            builder.Services.AddAppService();

            //Telegram
            builder.Services.AddTelegramBotClient();

            //Web API
            builder.Services.AddControllers();

            //Swagger
            builder.Services.AddSwagger();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
