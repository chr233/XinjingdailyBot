using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace XinjingDaily.Bot.Generator;

[Generator]
public class SugarTableIncrementalGenerator : IIncrementalGenerator
{
    // SqlSugar 特性完整名称
    private const string SugarTableAttributeFullName = "SqlSugar.SugarTableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. 筛选：所有带特性的类
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateClass(node),
                transform: static (ctx, _) => GetClassSymbol(ctx))
            .Where(symbol => symbol is not null); // 过滤无效类

        // 2. 收集：只保留标记了 [SugarTable] 的类
        var sugarTables = classDeclarations
            .Where(symbol =>
                symbol!.GetAttributes()
                      .Any(attr => attr.AttributeClass?.ToDisplayString() == SugarTableAttributeFullName));

        // 3. 组合并生成代码
        context.RegisterSourceOutput(sugarTables.Collect(), Execute);
    }

    /// <summary>
    /// 语法过滤：快速判断是否是【带特性的类】
    /// </summary>
    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl
               && classDecl.AttributeLists.Count > 0; // 有特性才处理
    }

    /// <summary>
    /// 获取类的完整符号信息（命名空间+类名）
    /// </summary>
    private static INamedTypeSymbol? GetClassSymbol(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
    }

    /// <summary>
    /// 生成最终代码
    /// </summary>
    private void Execute(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> tableTypes)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif

        if (tableTypes.IsEmpty) return;

        var code = new StringBuilder();
        code.AppendLine("// 自动生成：SqlSugar 自动创建表代码");
        code.AppendLine("using SqlSugar;");
        code.AppendLine();
        code.AppendLine("namespace SqlSugarAutoTable");
        code.AppendLine("{");
        code.AppendLine("    public static class AutoCreateTables");
        code.AppendLine("    {");
        code.AppendLine("        /// <summary>");
        code.AppendLine("        /// 自动创建所有 [SugarTable] 实体表");
        code.AppendLine("        /// </summary>");
        code.AppendLine("        public static void CreateAllTables(this ISqlSugarClient db)");
        code.AppendLine("        {");

        // 生成 SafeCreateTable 代码
        foreach (var type in tableTypes)
        {
            var fullName = type.ToDisplayString(); // 完整命名空间+类名
            code.AppendLine($"            db.CodeFirst.SafeCreateTable<{fullName}>();");
        }

        code.AppendLine("        }");
        code.AppendLine("    }");
        code.AppendLine("}");

        // 添加到编译
        context.AddSource(
            "AutoCreateTables.g.cs",
            SourceText.From(code.ToString(), Encoding.UTF8));
    }
}