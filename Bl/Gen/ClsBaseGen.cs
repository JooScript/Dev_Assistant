using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;
using Utils.Db;
using Utils.FileActions;
using Utils.Format;
using Utils.General;
using Utils.Validate;

namespace Bl.Gen;

public class ClsBaseGen
{
    public ClsBaseGen(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
        }

        InitializeConnectionString(DASettings.connStr);
        TableName = tableName;
    }

    public static void InitializeConnectionString(string connectionString) => DatabaseHelper.Initialize(connectionString);

    public static void ClearSchemaCache() => DatabaseHelper.ClearSchemaCache();

    public static string FormatId(string? input, bool smallD = true)
    {
        if (input == null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        if (input.EndsWith("id", StringComparison.OrdinalIgnoreCase) && input.Length >= 2)
        {
            char[] chars = input.ToCharArray();

            chars[^2] = 'I';
            chars[^1] = smallD ? 'd' : 'D';

            return new string(chars);
        }

        return input;
    }

    #region Props

    private static Regex _namingRegex => new Regex("_([A-Za-z])", RegexOptions.Compiled);

    public static IReadOnlyDictionary<string, DatabaseHelper.TableSchema> DatabaseSchema => DatabaseHelper.GetDatabaseSchema(true);

    protected static string TableName { get; set; }

    public static DatabaseHelper.TableSchema CurrentTableSchema => DatabaseSchema[TableName];

    public static DatabaseHelper.ColumnInfo PrimaryKeyCol => CurrentTableSchema.Columns.FirstOrDefault(x => x.IsPrimaryKey);

    protected static string TableId => FormatId(PrimaryKeyCol.Name);

    protected static string TableIdDT => Helper.GetCSharpType(PrimaryKeyCol.DataType);

    protected static string FormattedTableId => FormatHelper.CapitalizeFirstChars(_namingRegex.Replace(TableId, m => m.Groups[1].Value.ToUpper()));

    public static string[] Prefixes => new string[] { "Tbl_", "Tb_", "Tb" }.OrderByDescending(s => s.Length).ToArray();

    public static string ModelName
    {
        get
        {
            if (string.IsNullOrEmpty(TableName))
            {
                return null;
            }

            string tableName = FormatHelper.Singularize(WithoutPrefixFormattedTN);

            foreach (string item in Prefixes)
            {
                if (TableName.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    tableName = FormatHelper.CapitalizeFirstChars(_namingRegex.Replace($"{item}{tableName}", m => m.Groups[1].Value.ToUpper()));
                    break;
                }
            }

            return tableName;
        }
    }

    public static string WithoutPrefixFormattedTN
    {
        get
        {
            if (string.IsNullOrEmpty(TableName))
            {
                return null;
            }

            string tableName = TableName;

            foreach (string item in Prefixes)
            {
                if (tableName.StartsWith(item, StringComparison.OrdinalIgnoreCase))
                {
                    tableName = FormatHelper.CapitalizeFirstChars(_namingRegex.Replace(tableName.Substring(item.Length), m => m.Groups[1].Value.ToUpper()));
                    break;
                }
            }

            return tableName;
        }
    }

    protected static class MethodNames
    {
        public static string GetById => "GetByIdAsync";
        public static string GetAll => "GetAllAsync";
        public static string GetPagedList => "GetPagedListAsync";
        public static string Add => "AddAsync";
        public static string Update => "UpdateAsync";
        public static string IsExists => "IsExistsAsync";
        public static string Count => "CountAsync";
        public static string ChangeStatus => "ChangeStatusAsync";

    }

    protected static string ServiceClsName => $"{FormattedTNSingle}Service";

    protected static string DtoClsName => $"{FormattedTNSingle}Dto";

    protected static string LogicInterfaceName => $"I{FormattedTNSingle}";

    protected static string DataClsName => $"{FormattedTNSingle}Data";

    protected static string LogicObjName => $"o{FormattedTNSingle}";

    protected static string FormattedTNSingle => FormatHelper.CapitalizeFirstChars(FormatHelper.Singularize(WithoutPrefixFormattedTN) ?? string.Empty);

    protected static string FormattedTNSingleVar => FormatHelper.SmalizeFirstChar(FormattedTNSingle);

    protected static string FormattedTNPluralize => FormatHelper.CapitalizeFirstChars(FormatHelper.Pluralize(WithoutPrefixFormattedTN) ?? string.Empty);

    protected static string FormattedTNPluralizeVar => FormatHelper.SmalizeFirstChar(FormattedTNPluralize);

    protected static string AppName => _namingRegex.Replace(DASettings.AppName(), m => m.Groups[1].Value.ToUpper());

    public static string MappingTxt => "Mapping.txt";

    public static string BlDiTxt => "DI.txt";

    protected static List<DatabaseHelper.ColumnInfo> Columns => DatabaseHelper.GetTableColumns(TableName);

    public static string StoringPath
    {
        get
        {
            string fullPath = Path.Combine(FileHelper.GetPath(FileHelper.enSpecialFolderType.Desktop), "Code Generator", AppName);

            Helper.CreateFolderIfDoesNotExist(fullPath);

            return fullPath;
        }
    }

    #endregion

    /// <summary>
    /// Validates if a database table meets the necessary conditions for code generation.
    /// The table must exist, have columns, contain exactly one primary key that is an identity column of type int or bigint.
    /// </summary>
    /// <param name="tableName">Name of the database table to validate</param>
    /// <returns>
    /// True if the table meets all generation conditions:
    /// - Table exists
    /// - Table has columns
    /// - Table has exactly one primary key
    /// - Primary key is an identity column
    /// - Primary key is of type int or bigint
    /// Returns false and logs appropriate error messages if any condition fails.
    /// </returns>

    public bool CheckGeneratorConditions()
    {
        try
        {
            if (CurrentTableSchema == null)
            {
                FileHelper.ErrorLogger(new Exception($"Table '{TableName}' does not exist in the database."));
                return false;
            }

            if (!ValidationHelper.IsPlural(TableName))
            {
                FileHelper.ErrorLogger(new Exception($"Table '{TableName}' not Plural."));
                return false;
            }

            var columns = CurrentTableSchema.Columns;
            if (columns == null || columns.Count == 0)
            {
                FileHelper.ErrorLogger(new Exception($"Table '{TableName}' has no columns."));
                return false;
            }

            List<string> primaryKeys = CurrentTableSchema.PrimaryKeys;
            if (primaryKeys.Count != 1)
            {
                FileHelper.ErrorLogger(new Exception($"Table '{TableName}' must have exactly one primary key to generate code. Found {primaryKeys.Count}."));
                return false;
            }

            string primaryKey = primaryKeys[0];
            var primaryKeyColumn = columns.FirstOrDefault(col => col.Name == primaryKey);

            if (primaryKeyColumn == null)
            {
                FileHelper.ErrorLogger(new Exception($"Primary key '{primaryKey}' not found in table columns for table '{TableName}'."));
                return false;
            }

            if (primaryKeyColumn.DataType != "uniqueidentifier")
            {
                FileHelper.ErrorLogger(new Exception($"Primary key '{primaryKey}' in table '{TableName}' must be of type 'uniqueidentifier' to generate code. Found '{primaryKeyColumn.DataType}'."));
                return false;
            }

            if (primaryKeyColumn.Name != "Id")
            {
                FileHelper.ErrorLogger(new Exception($"Primary key '{primaryKey}' in table '{TableName}' should be named Id. Found '{primaryKeyColumn.Name}'."));
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            FileHelper.ErrorLogger(new Exception($"Error while validating table '{TableName}' for code generation: {ex.Message}", ex));
            return false;
        }
    }



}
