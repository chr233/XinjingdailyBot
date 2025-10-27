using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using XinjingdailyBot.Generator.Data;

namespace XinjingdailyBot.Generator;

[Generator]
internal sealed class ScheduleGenerator : IIncrementalGenerator
{
    const string InputFileName = "schedule.json";
    const string OutoutFileName = "GeneratedScheduleExtensions.g.cs";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(InputFileName))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString())
            .Where(text => text != null);

        context.RegisterSourceOutput(additionalFiles, (context, fileText) => {
            try
            {
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
        });
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="fileText"></param>
    /// <param name="context"></param>
    private void ProcessSettingsFile(string? fileText, SourceProductionContext context)
    {
        if (string.IsNullOrEmpty(fileText))
        {
            Debug.WriteLine("文件为空");
            return;
        }

        var json = JsonConvert.DeserializeObject<ScheduleData>(fileText) ?? throw new FileLoadException("文件读取失败");

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

        Debug.WriteLine(sb.ToString());

        context.AddSource(OutoutFileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

