using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Text;
using XinjingdailyBot.Generator.Data;

namespace XinjingdailyBot.Generator;

[Generator]
internal class AppServiceGenerator : ISourceGenerator
{
    const string InputFileName = "appService.json";
    const string OutoutFileName = "GeneratedAppServiceExtensions.g.cs";

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var fileText = context.AdditionalFiles.Where(static x => x.Path.EndsWith(InputFileName)).FirstOrDefault()
                ?? throw new FileNotFoundException("缺少配置文件, 请使用 scan_service.ps1 生成");

            ProcessSettingsFile(fileText, context);
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(
             Diagnostic.Create(
                 "XJB_01",
                 nameof(AppServiceGenerator),
                 $"生成注入代码失败，{e.Message}",
                 defaultSeverity: DiagnosticSeverity.Error,
                 severity: DiagnosticSeverity.Error,
                 isEnabledByDefault: true,
                 warningLevel: 0));
        }
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="xmlFile"></param>
    /// <param name="context"></param>
    private void ProcessSettingsFile(AdditionalText xmlFile, GeneratorExecutionContext context)
    {
        var text = xmlFile.GetText(context.CancellationToken)?.ToString() ?? throw new FileLoadException("文件读取失败");
        var json = JsonConvert.DeserializeObject<AppServiceData>(text) ?? throw new FileLoadException("文件读取失败");

        var sb = new StringBuilder();
        sb.AppendLine(Static.AppServiceHeader);

        foreach (var kv in json)
        {
            var entry = kv.Value;

            var lifeTime = entry.LifeTime?.ToLowerInvariant() switch {
                "singleton" or
                "scoped" or
                "transient" => entry.LifeTime,
                _ => null,
            };

            if (string.IsNullOrEmpty(lifeTime))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(entry.Class))
            {
                if (string.IsNullOrEmpty(entry.Interface))
                {
                    sb.AppendLine(string.Format(Static.AppServiceContent1, lifeTime, entry.Class));
                }
                else
                {
                    sb.AppendLine(string.Format(Static.AppServiceContent2, lifeTime, entry.Interface, entry.Class));
                }
            }
        }
        sb.AppendLine(Static.AppServiceFooter);

        context.AddSource(OutoutFileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

