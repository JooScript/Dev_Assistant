using Bl;
using Utils.ConsoleDisplay;
using Utils.Db;
using Utils.FileActions;
using Utils.General;

namespace DevAssistant;

public static class DbBackup
{
    public static async Task StartAsync()
    {
        try
        {
            Console.Clear();
            ConsoleHelper.PrintSectionHeader("Database Backup");

            DatabaseHelper.Initialize(DASettings.connStr);
            var databasesList = DatabaseHelper.ListDatabases();

            if (databasesList.Count == 0)
            {
                ConsoleHelper.ShowError("No databases found.");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            ConsoleHelper.ListConsolePrinting(databasesList);
            Console.ResetColor();

            string baseBackupPath = Path.Combine(
                            FileHelper.GetPath(FileHelper.enSpecialFolderType.Desktop),
                            "Database Backups",
                            $"Database Backup {DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
                        );

            foreach (string dbName in databasesList)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\nBacking up '{dbName}'...");
                    Console.ResetColor();

                    string backupPath = baseBackupPath;

                    if (!Helper.CreateFolderIfDoesNotExist(backupPath))
                    {
                        ConsoleHelper.ShowError($"Failed to create backup folder at {backupPath}");
                        Console.WriteLine("Press any key to return to the main menu...");
                        Console.ReadKey();
                        Console.ResetColor();
                        return;
                    }

                    DatabaseHelper.Initialize($"Server=.;Database={dbName};Integrated Security=SSPI;TrustServerCertificate=True;");
                    DatabaseHelper.BackupDatabase(ref backupPath);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"- Database '{dbName}' backed up at {backupPath}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    ConsoleHelper.ShowError($"Failed to backup '{dbName}': {ex.Message}");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\n-----------------------------");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✔ All databases processed successfully.");
            Console.WriteLine($"Backup files are located at:{baseBackupPath}");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            ConsoleHelper.ShowError($"Critical error: {ex.Message}");
        }
    }

}
