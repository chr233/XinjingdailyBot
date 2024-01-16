using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using XinjingdailyBot.Infrastructure.Attribute;

namespace XinjingdailyBot.Generator;

/// <summary>
/// 服务注册生成器
/// </summary>
//[Generator]
//public class CommandGenerator : ISourceGenerator
//{
//    private readonly Dictionary<Type, LifeTime> ServiceDict = [];

//    private void GetAllTypes(GeneratorExecutionContext 
//        context)
//    {
//        // 获取当前程序集的所有引用
//        var referencedAssemblies = context.Compilation.ReferencedAssemblyNames;

//        // 获取当前程序集的所有类型
//        var allTypes = new List<Type>();

//        // 获取当前程序集中的所有类型
//        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
//        {
//            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
//            var root = syntaxTree.GetRoot();

//            Console.WriteLine(root.ToString());

//            var classes = context.Compilation
//                    .SyntaxTrees
//                    .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
//                    .Where(x => x is ClassDeclarationSyntax)
//                    .Cast<ClassDeclarationSyntax>()
//                    .ToImmutableList();

//            var types = classes.Select(static x => x.GetType());

//        }

//        // 获取所有引用程序集中的类型
//        foreach (var referencedAssembly in referencedAssemblies)
//        {
//            //var referencedAssemblySymbol = context.Compilation.GetAssemblyOrModuleSymbol(referencedAssembly);
//            //var typesInReferencedAssembly = referencedAssemblySymbol?.GlobalNamespace.GetNamespaceMembers()
//            //    .SelectMany(namespaceSymbol => namespaceSymbol.GetTypeMembers())
//            //    .Where(typeSymbol => typeSymbol is not null && typeSymbol is ITypeSymbol)
//            //    .Select(typeSymbol => (ITypeSymbol)typeSymbol);

//            //allTypes.AddRange(typesInReferencedAssembly.Select(typeSymbol => typeSymbol.ToType()));
//        }
//    }

//    /// <inheritdoc />
//    public void Initialize(GeneratorInitializationContext context)
//    {
//#if DEBUGGENERATOR
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif
//    }

//    /// <inheritdoc />
//    public void Execute(GeneratorExecutionContext context)
//    {
//        //GetAllTypes(context);

//        //Console.WriteLine(context.ToString());
//    }
//}
