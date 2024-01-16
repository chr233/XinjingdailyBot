using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Text;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Generator;

/// <summary>
/// 服务注册生成器
/// </summary>
//[Generator]
//public class AppServiceGenerator : ISourceGenerator
//{
//    private readonly Dictionary<Type, LifeTime> ServiceDict = [];

//    /// <inheritdoc />
//    public void Execute(GeneratorExecutionContext context)
//    {
//        var rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
//        foreach ((string name, string template, string hash) in rx.TemplateInfo)
//        {
//            //string source = SourceFileFromMustachePath(name, template, hash);
//            //context.AddSource($"Mustache{name}.g.cs", source);
//        }
//    }
//    static string SourceFileFromMustachePath(string name, string template, string hash)
//    {
//        //        Func<object, string> tree = HandlebarsDotNet.Handlebars.Compile(template);
//        //        object? @object = Newtonsoft.Json.JsonConvert.DeserializeObject(hash);
//        //        if (@object is null)
//        //        {
//        //            return string.Empty;
//        //        }

//        //        string mustacheText = tree(@object);

//        //        var sb = new StringBuilder();
//        //        sb.Append($@"
//        //namespace Mustache {{

//        //    public static partial class Constants {{

//        //        public const string {name} = @""{mustacheText.Replace("\"", "\"\"")}"";
//        //    }}
//        //}}
//        //");
//        //        return sb.ToString();

//        return "";
//    }

//    /// <inheritdoc />
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//    }

//    class SyntaxReceiver : ISyntaxContextReceiver
//    {
//        public List<(string name, string template, string hash)> TemplateInfo = new List<(string name, string template, string hash)>();

//        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
//        {
//            if (context.Node is AttributeSyntax attrib)
//            {
//                Console.WriteLine(context.SemanticModel.GetTypeInfo(attrib).Type?.ToDisplayString());
//            }
//            else
//            {
//                Console.WriteLine(context.Node);
//            }
//            // find all valid mustache attributes
//            //if (context.Node is AttributeSyntax attrib
//            //    && attrib.ArgumentList?.Arguments.Count == 3
//            //    && context.SemanticModel.GetTypeInfo(attrib).Type?.ToDisplayString() == "MustacheAttribute")
//            //{
//            //    string name = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[0].Expression).ToString();
//            //    string template = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[1].Expression).ToString();
//            //    string hash = context.SemanticModel.GetConstantValue(attrib.ArgumentList.Arguments[2].Expression).ToString();

//            //    TemplateInfo.Add((name, template, hash));
//            //}
//        }
//    }
//}



using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

using System.Linq;
using System.Text;

[Generator]
public class AppServiceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // 初始化逻辑
        context.RegisterForSyntaxNotifications(() => new ControllerFinder());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var symbol in context.Compilation.SourceModule.ReferencedAssemblySymbols)
        {
            foreach(var x in symbol.gets)
            Console.WriteLine(syntaxTree.FilePath);
        }
    }

    private bool HasAppServiceAttribute(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "AppServiceAttribute");
    }

    private string GenerateCode(List<INamedTypeSymbol> appServiceClasses)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("namespace YourNamespace");
        stringBuilder.AppendLine("{");
        stringBuilder.AppendLine("    public static class ServiceRegistration");
        stringBuilder.AppendLine("    {");
        stringBuilder.AppendLine("        public static void RegisterServices(IServiceCollection services)");
        stringBuilder.AppendLine("        {");

        foreach (var appServiceClass in appServiceClasses)
        {
            stringBuilder.AppendLine($"            services.AddScoped<{appServiceClass.ToDisplayString()}>();");
        }

        stringBuilder.AppendLine("        }");
        stringBuilder.AppendLine("    }");
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();
    }


    public class ControllerFinder : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Controllers { get; }
            = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax controller)
            {
                //if (controller.Identifier.ValueText.StartsWith("Xinjingdaily"))
                //{
                Controllers.Add(controller);
                //}
            }
        }
    }
}
