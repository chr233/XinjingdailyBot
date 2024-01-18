using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XinjingdailyBot.Generator;

internal class TypeSyntaxReceiver : ISyntaxReceiver
{
    // 声明一个集合用来暂存接收到的类型语法
    public HashSet<TypeDeclarationSyntax> TypeDeclarationsWithAttributes { get; } = new();

    // 当编译器访问语法节点的时候会调用此方法
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        // 这里判断这个节点是不是类型声明的语法并且是否包含特性声明语法。我们只对打了特殊标记的进行处理，其余的就不处理。减少一些编译时的影响
        if (syntaxNode is TypeDeclarationSyntax declaration && declaration.AttributeLists.Any())
        {
            TypeDeclarationsWithAttributes.Add(declaration);
        }

        // 还有比如：MethodDeclarationSyntax 方法声明 InterfaceDeclarationSyntax接口声明等等，根据需要可以接受不同的语法结点进行分析。
    }

}
