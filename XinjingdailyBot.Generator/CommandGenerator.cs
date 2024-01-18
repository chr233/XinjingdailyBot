using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace XinjingdailyBot.Generator;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

//[Generator]
//public class ServiceRegistrationGenerator : ISourceGenerator
//{
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        // 注册对应的属性
//        context.RegisterForSyntaxNotifications(() => new AppServiceSyntaxReceiver());
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        // 获取属性接收器
//        if (!(context.SyntaxReceiver is AppServiceSyntaxReceiver syntaxReceiver))
//            return;

//        // 生成服务注册代码
//        var code = GenerateServiceRegistrationCode(syntaxReceiver.CandidateAttributes);

//        // 将生成的代码添加到输出
//        context.AddSource("ServiceRegistration.Generated.cs", SourceText.From(code, Encoding.UTF8));
//    }

//    private string GenerateServiceRegistrationCode(List<AttributeSyntax> attributes)
//    {
//        // 生成的代码的命名空间、类名等信息
//        var namespaceName = "Generated";
//        var className = "ServiceRegistration";

//        // 生成类的代码
//        var classDeclaration = SyntaxFactory.ClassDeclaration(className)
//            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
//            .AddMembers((MemberDeclarationSyntax)GenerateServiceRegistrationMethods(attributes));

//        // 生成命名空间的代码
//        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
//            .AddMembers(classDeclaration);

//        // 生成整个文件的代码
//        var compilationUnit = SyntaxFactory.CompilationUnit()
//            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")))
//            .AddMembers(namespaceDeclaration);

//        // 转换为字符串
//        var syntaxTree = SyntaxFactory.SyntaxTree(compilationUnit);
//        var formattedCode = syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

//        return formattedCode;
//    }

//    private IEnumerable<MemberDeclarationSyntax> GenerateServiceRegistrationMethods(List<AttributeSyntax> attributes)
//    {
//        // 生成注册服务的方法
//        foreach (var attribute in attributes)
//        {
//            // 解析属性的信息，根据需要生成注册代码
//            // 这里假设 AppServiceAttribute 有一个 Name 属性，表示服务的名称
//            var serviceName = attribute.ArgumentList.Arguments.First().Expression.ToString();

//            // 生成注册代码
//            var method = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "RegisterServices")
//                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
//                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)))
//                .AddBodyStatements(
//                    SyntaxFactory.ExpressionStatement(
//                        SyntaxFactory.ParseExpression($"services.AddTransient<{serviceName}>();")
//                    )
//                );

//            yield return method;
//        }
//    }
//}

//// 用于接收带有 AppServiceAttribute 的语法节点
//internal class AppServiceSyntaxReceiver : ISyntaxReceiver
//{
//    public List<AttributeSyntax> CandidateAttributes { get; } = new List<AttributeSyntax>();

//    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//    {
//        // 检查语法节点是否包含 AppServiceAttribute
//        if (syntaxNode is AttributeSyntax attributeSyntax
//            && attributeSyntax.Name.ToString() == "AppServiceAttribute")
//        {
//            CandidateAttributes.Add(attributeSyntax);
//        }
//    }
//}
