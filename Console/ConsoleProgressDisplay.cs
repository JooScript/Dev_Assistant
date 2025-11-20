using Bl.Gen;
using Utils.ConsoleDisplay;
using static Bl.Gen.ClsGenPublisher;

namespace DevAssistant;

/// <summary>
/// Console subscriber for ClsGenerator progress updates with enhanced formatting.
/// </summary>
public class CodeGenerationConsoleSubscriber
{
    private readonly ClsGenPublisher _generator;
    private int _totalTables = 0;
    private int _currentIndex = 0;
    private enStep? _lastStepCategory = null;

    public CodeGenerationConsoleSubscriber(ClsGenPublisher generator, int totalTables = 0)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _totalTables = totalTables;
        _generator.ProgressUpdated += OnProgressUpdated;
    }

    private void OnProgressUpdated(object sender, CodeGenEventArgs e)
    {
        if (_lastStepCategory != e.Step)
        {
            ConsoleHelper.PrintSectionHeader(GetCategoryName(e.Step));
            _lastStepCategory = e.Step;
        }

        if (e.Step == enStep.GeneratingTable && !string.IsNullOrEmpty(e.TableName) && e.Message.StartsWith("Generating code", StringComparison.OrdinalIgnoreCase))
        {
            _currentIndex++;
        }

        var originalColor = Console.ForegroundColor;

        if (!string.IsNullOrEmpty(e.TableName))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (_totalTables > 0)
                Console.Write($"[{_currentIndex:D2}/{_totalTables:D2}] ");
            Console.Write($"{e.TableName,-35}");
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"[{e.Step}] ");

        Console.ForegroundColor = e.Success ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(e.Message);

        Console.ForegroundColor = originalColor;
    }

    private static string GetCategoryName(enStep step) =>
        step switch
        {
            enStep.LoadingSchema => "Schema Loading",
            enStep.LoadingSchemaRetrying => "Schema Retrying",
            enStep.CheckingConditions => "Checking Conditions",
            enStep.GeneratingTable => "Code Generation",
            enStep.CondCheckFailed => "Condition Check Failed",
            _ => "Other"
        };

    public void Unsubscribe()
    {
        _generator.ProgressUpdated -= OnProgressUpdated;
    }

}
