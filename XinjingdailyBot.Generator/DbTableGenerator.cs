using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using XinjingdailyBot.Generator.Data;

namespace XinjingdailyBot.Generator;

[Generator]
internal sealed class DbTableGenerator : IIncrementalGenerator
{
    const string InputFileName = "dbtable.json";
    const string OutoutFileName = "GeneratedDbTableExtensions.g.cs";

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
                        "XJB_03",
                        nameof(AppServiceGenerator),
                        $"生成注入代码失败，{e.Message}",
                        defaultSeverity: DiagnosticSeverity.Error,
                        severity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0));
            }
        });
    }

    private void ProcessSettingsFile(string? fileText, SourceProductionContext context)
    {
        if (string.IsNullOrEmpty(fileText))
        {
            Debug.WriteLine("文件为空");
            return;
        }

        var json = JsonConvert.DeserializeObject<DbTableData>(fileText) ?? throw new FileLoadException("文件读取失败");

        var sb = new StringBuilder();
        sb.AppendLine(Templates.DbTableHeader);

        foreach (var kv in json)
        {
            var name = kv.Key;
            var className = kv.Value;

            if (string.IsNullOrEmpty(className))
            {
                continue;
            }

            sb.AppendLine(string.Format(Templates.DbTableContent, name, className));
        }
        sb.AppendLine(Templates.DbTableFooter);

        Debug.WriteLine(sb.ToString());

        context.AddSource(OutoutFileName, SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

