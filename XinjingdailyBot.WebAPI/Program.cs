
using NLog.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using XinjingdailyBot.Infrastructure;
using XinjingdailyBot.WebAPI.Extensions;

namespace XinjingdailyBot.WebAPI
{
    /// <summary>
    /// 根程序集
    /// </summary>
    public static class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 启动入口
        /// </summary>
        /// <param name="args"></param>
        [RequiresUnreferencedCode("不兼容剪裁")]
        public static void Main(string[] args)
        {
            _logger.Info("欢迎使用 XinjingdailyBot Version: {0} - {1}", Utils.Version, BuildInfo.Variant);

            var builder = WebApplication.CreateBuilder(args);

            //配置类支持
            builder.Services.AddOptions().Configure<OptionsSetting>(builder.Configuration);

            //NLog
            builder.Services.AddLogging(loggingBuilder => {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                loggingBuilder.AddNLog();
            });

            //SqlSugar
            builder.Services.AddSqlSugar(builder.Configuration);

            //添加服务
            builder.Services.AddAppService();

            //注册HttpClient
            builder.Services.AddHttpClients();

            //Telegram
            builder.Services.AddTelegramBotClient();

            //定时任务
            builder.Services.AddTasks();

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

            CleanOldFiles();

            app.Run();
        }

        /// <summary>
        /// 清除升级文件
        /// </summary>
        private static void CleanOldFiles()
        {
            string bakPath = Utils.BackupFullPath;
            if (File.Exists(bakPath))
            {
                try
                {
                    File.Delete(bakPath);
                    _logger.Warn("清理升级残留文件");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "清理升级残留文件失败");
                }
            }
        }
    }
}
