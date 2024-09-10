using Spectre.Console;

namespace XinjingdailyBot.Setup;

class Program
{
    static void Main(string[] args)
    {
        var menuItems = new[] { "选项 1", "选项 2", "选项 3", "退出" };
        var selectedIndex = 0;

        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(
                new Panel(new Markup("[bold yellow]请选择一个选项:[/]"))
                    .Expand()
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Style.Parse("blue"))
                    .Header("[bold blue]菜单[/]")
            );

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == selectedIndex)
                {
                    AnsiConsole.MarkupLine($"[bold green]> {menuItems[i]}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"  {menuItems[i]}");
                }
            }

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                    break;
                case ConsoleKey.Enter:
                    if (menuItems[selectedIndex] == "退出")
                    {
                        return;
                    }
                    AnsiConsole.MarkupLine($"你选择了: [bold yellow]{menuItems[selectedIndex]}[/]");
                    AnsiConsole.MarkupLine("按任意键返回菜单...");
                    Console.ReadKey(true);
                    break;
            }
        }
    }
}
