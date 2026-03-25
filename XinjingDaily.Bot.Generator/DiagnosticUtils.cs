using Microsoft.CodeAnalysis;

namespace XinjingDaily.Bot.Generator;

internal static class DiagnosticUtils
{
    private const string Category = "XinjingDaily.Bot.Generator";

    public static DiagnosticDescriptor EntryNotFoundDiagnostic = new DiagnosticDescriptor(
        id: "XJDB_GEN_001",
        title: "未找到任何 Entry 类",
        messageFormat: "未找到任何带有 [SugarTable] 特性的类，请确保至少有一个类被标记为 [SugarTable]",
        category: Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
