namespace XinjingdailyBot.Generator;
//internal sealed class PlatformInfoGenerator : ISourceGenerator
//{
//    const string OutoutFileName = "BuildConfig.g.cs";

//    /// <inheritdoc/>
//    public void Initialize(GeneratorInitializationContext context)
//    {
//        // 无需处理
//#if DEBUG
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif
//    }

//    /// <inheritdoc/>
//    public void Execute(GeneratorExecutionContext context)
//    {
//        // 获取编译的目标平台类型
//        var targetPlatform = "";
//        foreach (var opt in context.Compilation.Options.SpecificDiagnosticOptions)
//        {
//            Debug.WriteLine("{0} : {1}", opt.Key, opt.Value);
//        }

//        // 生成代码
//        var code = $@"
//namespace Generated
//{{
//    public class PlatformInfo
//    {{
//        public static string GetTargetPlatform()
//        {{
//            return ""{targetPlatform}"";
//        }}
//    }}
//}}
//";

//        // 添加生成的代码到 Compilation
//        //context.AddSource(OutoutFileName, SourceText.From(code, Encoding.UTF8));
//    }
//}
