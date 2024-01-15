using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Generator;

/// <summary>
/// 服务注册生成器
/// </summary>
[Generator]
public class AppServiceGenerator : ISourceGenerator
{
    private readonly Dictionary<Type, LifeTime> ServiceDict = [];

    private void GetAllTypes(GeneratorExecutionContext context)
    {
        // 获取当前程序集的所有引用
        var referencedAssemblies = context.Compilation.ReferencedAssemblyNames;

        // 获取当前程序集的所有类型
        var allTypes = new List<Type>();

        // 获取当前程序集中的所有类型
        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            Console.WriteLine(root.ToString());

            // 使用语法树的语义模型获取类型
            var typesInSyntaxTree = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Select(classSyntax => semanticModel.GetDeclaredSymbol(classSyntax))
                .Where(symbol => symbol is not null && symbol is ITypeSymbol)
                .Select(symbol => (ITypeSymbol?)symbol);

            //var x = typesInSyntaxTree.Select(typeSymbol => typeSymbol?.IsType == true);


            var controllers = context.Compilation
                    .SyntaxTrees
                    .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
                    .Where(x => x is ClassDeclarationSyntax)
                    .Cast<ClassDeclarationSyntax>()
                    .Where(c => c.Identifier.ValueText.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                    .ToImmutableList();


        }

        // 获取所有引用程序集中的类型
        foreach (var referencedAssembly in referencedAssemblies)
        {
            //var referencedAssemblySymbol = context.Compilation.GetAssemblyOrModuleSymbol(referencedAssembly);
            //var typesInReferencedAssembly = referencedAssemblySymbol?.GlobalNamespace.GetNamespaceMembers()
            //    .SelectMany(namespaceSymbol => namespaceSymbol.GetTypeMembers())
            //    .Where(typeSymbol => typeSymbol is not null && typeSymbol is ITypeSymbol)
            //    .Select(typeSymbol => (ITypeSymbol)typeSymbol);

            //allTypes.AddRange(typesInReferencedAssembly.Select(typeSymbol => typeSymbol.ToType()));
        }
    }

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGENERATOR
        if (!Debugger.IsAttached)
        {
            Debugger.Launch();
        }
#endif
    }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        GetAllTypes(context);

        Console.WriteLine(context.ToString());
    }
}
