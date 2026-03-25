using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XinjingDaily.Bot.Generator;

internal static class Utils
{
    /// <summary>
    /// 判断是否带有指定 Attribute
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <param name="helloAttributeSymbol"></param>
    /// <returns></returns>
    public static bool HasTargetAttribute(INamedTypeSymbol? classSymbol, INamedTypeSymbol? helloAttributeSymbol)
    {
        if (classSymbol == null)
        {
            return false;
        }

        return classSymbol.GetAttributes().Any(attr =>
            SymbolEqualityComparer.Default.Equals(attr.AttributeClass, helloAttributeSymbol));
    }

    /// <summary>
    /// 获取类的完整符号信息（命名空间+类名）
    /// </summary>
    public static INamedTypeSymbol? GetClassOrRecordSymbol(GeneratorSyntaxContext context)
    {
        // 支持 class
        if (context.Node is ClassDeclarationSyntax classDecl)
        {
            return context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        }

        // 支持 record
        if (context.Node is RecordDeclarationSyntax recordDecl)
        {
            return context.SemanticModel.GetDeclaredSymbol(recordDecl) as INamedTypeSymbol;
        }

        return null;
    }
}
