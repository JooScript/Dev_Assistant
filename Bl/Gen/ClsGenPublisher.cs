using Utils.FileActions;
using Utils.Format;
using Utils.General;

namespace Bl.Gen;

public class ClsGenPublisher
{
    public enum enStep
    {
        GeneratingTable,
        CheckingConditions,
        LoadingSchemaRetrying,
        LoadingSchema,
        CondCheckFailed
    }

    public class CodeGenEventArgs : EventArgs
    {
        public string TableName { get; set; }
        public enStep Step { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Current { get; set; }
        public int Total { get; set; }
    }

    public event EventHandler<CodeGenEventArgs> ProgressUpdated;

    delegate bool GenMethod(out string path);

    public class TOptions
    {
        public string Path { get; set; } = null!;
        public bool AllowCopy { get; set; } = false;
    }

    public class CodeGenOptions
    {
        public TOptions Logic { get; set; }
        public TOptions BlContract { get; set; }
        public TOptions Dto { get; set; }
        public TOptions Controller { get; set; }
    }

    private bool CheckConditions(ref List<string> tables)
    {
        List<string> Excluded = new List<string> { "__EFMigrationsHistory", "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins", "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens", "Logs" };
        ClsBaseGen.InitializeConnectionString(DASettings.ConnectionString());
        bool allCondsSuccess = false;
        int retryingCounter = 0;
        int retryingNumber = 3;
        bool isFirst = true;

        while (!allCondsSuccess)
        {
            if (retryingCounter > retryingNumber)
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Message = "Tables do not fulfill conditions.",
                    Success = false,
                    Step = enStep.CondCheckFailed
                });
                return false;
            }

            if (isFirst)
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Step = enStep.LoadingSchema,
                    Success = true
                });
                isFirst = false;
            }
            else
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Message = $"- Retrying [{retryingCounter}/{retryingNumber}]... ",
                    Step = enStep.LoadingSchemaRetrying,
                    Success = true
                });
            }

            ClsBaseGen.ClearSchemaCache();

            tables = ClsBaseGen.DatabaseSchema.Keys.ToList().Except(Excluded).ToList();

            int condCounter = tables.Count;

            if (tables == null || condCounter == 0)
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Message = "No tables found in the database.",
                    Success = false
                });
                return false;
            }

            int formatCounter = 0;
            retryingCounter++;

            foreach (var table in tables)
            {
                condCounter--;
                formatCounter++;

                string condFormattedCounter = FormatHelper.FormatNumbers(formatCounter, tables.Count);

                bool condSuccess = new ClsBaseGen(table).CheckGeneratorConditions();
                allCondsSuccess = condSuccess && condCounter == 0;
                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Step = enStep.CheckingConditions,
                    Message = $"{condFormattedCounter}- Checking Conditions for {table}...",
                    Success = condSuccess
                });
            }
        }

        return true;
    }

    private bool CreateCode(GenMethod creationMethod, TOptions des)
    {
        string src = string.Empty;
        bool success = creationMethod(out src);

        if (Helper.CreateFolderIfDoesNotExist(des.Path) && des.AllowCopy)
        {
            Helper.DeleteFolder(des.Path, true);
            FileHelper.CopyFileToFolder(des.Path, ref src, false, true);
        }

        return success;
    }

    public void GenerateCode(CodeGenOptions options)
    {
        List<string> tables = new();

        try
        {
            if (!CheckConditions(ref tables)) return;

            short counter = 0;
            bool allSuccess = true;
            List<string> failedTables = new List<string>();

            foreach (string table in tables)
            {
                counter++;

                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Step = enStep.GeneratingTable,
                    Message = $"Generating code for: {table} ({counter}/{tables.Count})",
                    Success = true
                });

                bool dtoSuccess = CreateCode(new ClsDtoGen(table).GenerateDTO, options.Dto);
                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Message = $"DTO generation for {table} {(dtoSuccess ? "succeeded" : "failed")}.",
                    Success = dtoSuccess
                });

                bool blSuccess = CreateCode(new ClsGen(table).GenerateBlCode, options.Logic);
                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Message = $"BL generation for {table} {(blSuccess ? "succeeded" : "failed")}.",
                    Success = blSuccess
                });

                bool bliSuccess = CreateCode(new ClsGen(table).GenerateContractsCode, options.BlContract);
                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Message = $"BLI generation for {table} {(bliSuccess ? "succeeded" : "failed")}.",
                    Success = bliSuccess
                });

                bool apiSuccess = CreateCode(new ClsAPIGen(table).GenerateControllerCode, options.Controller);
                UpdateProgress(new CodeGenEventArgs
                {
                    TableName = table,
                    Message = $"API generation for {table} {(apiSuccess ? "succeeded" : "failed")}.",
                    Success = apiSuccess
                });

                if (!dtoSuccess || !blSuccess || !apiSuccess || !bliSuccess)
                {
                    failedTables.Add(table);
                    allSuccess = false;
                }
            }

            if (allSuccess)
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Message = "✓ Code generation completed successfully for all tables!",
                    Success = true
                });
            }
            else
            {
                UpdateProgress(new CodeGenEventArgs
                {
                    Message = $"❌ Code generation completed with {failedTables.Count} failures. Failed tables: {string.Join(", ", failedTables)}",
                    Success = false
                });
            }
        }
        catch (Exception ex)
        {
            FileHelper.ErrorLogger(ex);
            UpdateProgress(new CodeGenEventArgs
            {
                Message = $"❌ CRITICAL ERROR: Code generation process failed! Error: {ex.Message}",
                Success = false
            });
        }
    }

    public void UpdateProgress(CodeGenEventArgs e)
    {
        OnProgressUpdated(e);
    }

    private void OnProgressUpdated(CodeGenEventArgs e) => ProgressUpdated?.Invoke(this, e);

}