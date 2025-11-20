using Utils.ConsoleDisplay;

namespace DevAssistant;

internal partial class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Dev Asistant";
        Start();
    }

    enum enAssistantServices
    {
        None = 0,
        CodeGen = 1,
        DatabaseBackup = 2
    }

    private static enAssistantServices CurrentService = enAssistantServices.None;

    private static void Start()
    {
        while (true)
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Select Service");
            ConsoleHelper.WriteMenuOption(1, "Code Generation");
            ConsoleHelper.WriteMenuOption(2, "Database Backup");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Enter your choice (1-2): ");
            Console.ResetColor();

            if (!short.TryParse(Console.ReadLine(), out short choice) || choice < 1 || choice > 2)
            {
                ConsoleHelper.ShowError("Invalid input. Please enter 1 or 2.");
                continue;
            }

            CurrentService = (enAssistantServices)choice;
            switch (CurrentService)
            {
                case enAssistantServices.CodeGen:
                    {
                        CodeGen.Start();
                        break;
                    }
                case enAssistantServices.DatabaseBackup:
                    {
                        DbBackup.StartAsync();
                        break;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("No correct service selected");
                    }
            }
        }
    }

}
