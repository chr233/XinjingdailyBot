using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace XinjingdailyBot.Generator;

[Generator]
public class AppServiceGenerator : ISourceGenerator
{
    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached) // 这几句是附加调试的，加上这个会进入调试模式，和正常的c#代码调试一样。
        {
            Debugger.Launch();
        }
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            //获取Program.cs文件路径
            var programFilePath = context.Compilation.SyntaxTrees
                .Select(static x => x.FilePath)
                .Where(static x => x.EndsWith("Program.cs"))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(programFilePath))
            {
                throw new FileNotFoundException("找不到Program.cs文件");
            }

            var solutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(programFilePath), ".."));


#pragma warning disable RS1035 // 不要使用禁用于分析器的 API
            foreach(var subModulePath in Directory.GetDirectories(solutionPath))
            {
                Console.WriteLine(subModulePath);
            }
#pragma warning restore RS1035 // 不要使用禁用于分析器的 API

            Console.Write(1);

            // 这里就是具体的注入代码生成的地方，基本都是拼接字符串。最终生成IServiceCollection的扩展。
            //    var injectStr = $@" 
            //using Microsoft.Extensions.DependencyInjection;

            //namespace {injectCodeNamespace} {{
            //    public static class AutoInjectHelper
            //    {{
            //        public static IServiceCollection AutoInject{injectCodeNamespace.Replace(".", "_")}(this IServiceCollection service)
            //        {{";
            //    var sb = new StringBuilder(injectStr);

            //    foreach (var targetType in targetTypes)
            //    {
            //        context.CancellationToken.ThrowIfCancellationRequested();
            //        var proxySource = GenerateInjectCode(targetType, @namespace, attribute);
            //        sb.AppendLine(proxySource);
            //    }

            //    var end = $@"  return  service; }}
            //    }}
            //}}";
            //    sb.Append(end);


            //    context.AddSource($"AutoInjectHelper.Inject.cs", sb.ToString());

        }
        catch (Exception e)
        {
            context.ReportDiagnostic(
             Diagnostic.Create(
                 "XJB_01",
                 nameof(AppServiceGenerator),
                 $"生成注入代码失败，{e.Message}",
                 defaultSeverity: DiagnosticSeverity.Warning,
                 severity: DiagnosticSeverity.Warning,
                 isEnabledByDefault: true,
                 warningLevel: 1));
        }
    }
}

