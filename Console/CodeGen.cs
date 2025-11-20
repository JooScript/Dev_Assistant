using Bl.Gen;
using Utils.ConsoleDisplay;
using Utils.General;
using static Bl.Gen.ClsGenPublisher;

namespace DevAssistant;

public static class CodeGen
{
    public static void Start()
    {
        Console.Clear();
        ConsoleHelper.PrintSectionHeader("Code Generation");

        var generator = new ClsGenPublisher();
        var consoleSubscriber = new CodeGenerationConsoleSubscriber(generator);

        generator.GenerateCode(GetCodeOptions());

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✔ Code generation completed successfully.");
        Console.ResetColor();

        consoleSubscriber.Unsubscribe();
    }

    private static CodeGenOptions GetCodeOptions()
    {
        bool logicAllowCopy = GetGenAllowCopy("Do you want to copy Logic code files to a specific folder?");
        bool blContractAllowCopy = GetGenAllowCopy("Do you want to copy BL Contracts code files to a specific folder?");
        bool dtoAllowCopy = GetGenAllowCopy("Do you want to copy DTO code files to a specific folder?");
        bool controllerAllowCopy = GetGenAllowCopy("Do you want to copy Controller code files to a specific folder?");

        string logicPath = logicAllowCopy ? GetGenPath("Enter Logic Path") : "";
        string blContractPath = blContractAllowCopy ? GetGenPath("Enter BL Contract Path") : "";
        string dtoPath = dtoAllowCopy ? GetGenPath("Enter DTO Path") : "";
        string controllerPath = controllerAllowCopy ? GetGenPath("Enter Controller Path") : "";

        return new CodeGenOptions
        {
            Logic = new TOptions { Path = logicPath, AllowCopy = logicAllowCopy },
            BlContract = new TOptions { Path = blContractPath, AllowCopy = blContractAllowCopy },
            Dto = new TOptions { Path = dtoPath, AllowCopy = dtoAllowCopy },
            Controller = new TOptions { Path = controllerPath, AllowCopy = controllerAllowCopy }
        };
    }

    private static string GetGenPath(string title)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{title}");
            Console.WriteLine("⚠ Any content in the selected folder will be deleted.");
            Console.ResetColor();

            Console.Write("Path: ");
            var path = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(path))
            {
                ConsoleHelper.ShowError("Path cannot be empty.");
                continue;
            }

            if (Helper.CreateFolderIfDoesNotExist(path))
                return path;

            ConsoleHelper.ShowError("Unable to create or access the specified folder.");
        }
    }

    private static bool GetGenAllowCopy(string title)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{title} (Y/N): ");
            Console.ResetColor();

            var res = Console.ReadLine()?.Trim().ToLower();

            if (res == "n") return false;
            if (res == "y") return true;

            ConsoleHelper.ShowError("Invalid input. Please enter 'Y' or 'N'.");
        }
    }

}
