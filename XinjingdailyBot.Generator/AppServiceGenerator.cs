using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Generator;

[Generator]
public class NativeDependencyInjectGenerator : ISourceGenerator
{
    // 初始化
    public void Initialize(GeneratorInitializationContext context)
    {
        if (!Debugger.IsAttached) // 这几句是附加调试的，加上这个会进入调试模式，和正常的c#代码调试一样。
        {
            Debugger.Launch();
        }
        // 上下文注册语法接收器
        context.RegisterForSyntaxNotifications(() => new TypeSyntaxReceiver());
    }

    // 具体的执行方法
    public void Execute(GeneratorExecutionContext context)
    {
        // 这里获取当前编译上下文的编译对象，只会包含用户代码
        var compilation = context.Compilation;
        // 获取命名空间 这里使用当前编译的程序集名称，还可以获取入口程序所在的名称空间。但是我们是直接生成这个程序集下的所有声明未可注入的，所以就直接用程序集名称了。
        var injectCodeNamespace = compilation.Assembly.Name;

        // 这两句是从上下文中拿到我们需要分析的类型语法分析器，并拿到类型集合。
        var syntaxReceiver = (TypeSyntaxReceiver)context.SyntaxReceiver;
        var injectTargets = syntaxReceiver?.TypeDeclarationsWithAttributes;
        if (injectTargets == null || !injectTargets.Any())
            return;

        // 这里是生成可注入的特性代码，并添加到编译上下文。
        // 这里生成的代码最终是一个特性，只能标记在public class上，打了此标记的才会生成到注入代码中
        //var injectAttributeStr = GeneratorInjectAttributeCode(context, "XinjingdailyBot.Generator");

        // 这几行是将上边的特性添加到语法树，并获取原信息。后边要做处理
        //var options = (CSharpParseOptions)compilation.SyntaxTrees.First().Options;
        //var logSyntaxTree = CSharpSyntaxTree.ParseText(injectCodeStr, options);
        //compilation = compilation.AddSyntaxTrees(logSyntaxTree);
        var attribute = compilation.GetTypeByMetadataName("XinjingdailyBot.Infrastructure.Attribute.AppServiceAttribute");

        // 这里定义一个最终的分析类型集合，里面包含的就是有注入标记的类型。
        var targetTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var targetTypeSyntax in injectTargets)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(targetTypeSyntax.SyntaxTree);
            var targetType = semanticModel.GetDeclaredSymbol(targetTypeSyntax);
            if (targetType?.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var hasInjectAttribute = targetType?.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attribute)) ?? false;
            if (!hasInjectAttribute)
                continue;
            targetTypes.Add(targetType);
        }


        try
        {

            // 这里就是具体的注入代码生成的地方，基本都是拼接字符串。最终生成IServiceCollection的扩展。
            var injectStr = $@" 
using Microsoft.Extensions.DependencyInjection;

namespace {injectCodeNamespace} {{
    public static class AutoInjectHelper
    {{
        public static IServiceCollection AutoInject{injectCodeNamespace.Replace(".", "_")}(this IServiceCollection service)
        {{";
            var sb = new StringBuilder(injectStr);

            foreach (var targetType in targetTypes)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var proxySource = GenerateInjectCode(targetType, @namespace, attribute);
                sb.AppendLine(proxySource);
            }

            var end = $@"  return  service; }}
    }}
}}";
            sb.Append(end);


            context.AddSource($"AutoInjectHelper.Inject.cs", sb.ToString());
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    "AUTODI_01",
                    "DependencyInject Generator",
                    $"生成注入代码失败，{e.Message}",
                    defaultSeverity: DiagnosticSeverity.Error,
                    severity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true,
                    warningLevel: 0));
        }

    }
}

