using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Text;
using XinjingdailyBot.Generator.Data;

namespace XinjingdailyBot.Generator;

[Generator]
internal sealed class ScheduleGenerator : ISourceGenerator
{
    const string InputFileName = "schedule.json";
    const string OutoutFileName = "GeneratedScheduleExtensions.g.cs";

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        // 无需处理
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var fileText = context.AdditionalFiles.Where(static x => x.Path.EndsWith(InputFileName)).FirstOrDefault() ?? throw new FileNotFoundException("缺少配置文件, 请使用 scan_service.ps1 生成");

            ProcessSettingsFile(fileText, context);
        }
        catch (IOException e)
        {
            context.ReportDiagnostic(
             Diagnostic.Create(
                 "XJB_02",
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
        var json = JsonConvert.DeserializeObject<ScheduleData>(text) ?? throw new FileLoadException("文件读取失败");

        var sb = new StringBuilder();
        sb.AppendLine(Templates.ScheduleHeader);

        foreach (var kv in json)
        {
            var entry = kv.Value;

            var name = kv.Key;
            var schedule = entry.Schedule;
            var className = entry.Class;

            if (string.IsNullOrEmpty(schedule) || string.IsNullOrEmpty(className))
            {
                continue;
            }

            sb.AppendLine(string.Format(Templates.ScheduleContent, name, schedule, className));
        }
        sb.AppendLine(Templates.ScheduleFooter);

        Console.WriteLine(sb.ToString());

        context.AddSource(OutoutFileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }

}

