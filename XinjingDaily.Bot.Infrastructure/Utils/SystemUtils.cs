namespace XinjingDaily.Bot.Infrastructure.Utils;

public static class SystemUtils
{
    public static void Shutdown()
    {
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
        Environment.Exit(1);
    }
}
