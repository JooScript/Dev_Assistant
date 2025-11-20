using System.Text;
using Utils.FileActions;
using Utils.Format;
using Utils.General;

namespace Bl.Gen;

public class ClsDaGen : ClsBaseGen
{
    public ClsDaGen(string tableName) : base(tableName) { }

    #region  Class Structure

    private static string IsExistByUsernameMethod() =>
         $@"public static async Task<bool> Is{FormattedTNSingle}ExistsByUsernameAsync(string username)
        {{
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(""Username cannot be empty"", nameof(username));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Is{FormattedTNSingle}ExistsByUsername"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@Username"", username);
                    await connection.OpenAsync();

                    var returnParameter = command.Parameters.Add(""@ReturnVal"", SqlDbType.{GetSqlDbType()});
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await command.ExecuteNonQueryAsync();

                    return ({TableIdDT})returnParameter.Value == 1;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return false;
            }}
        }}

";

    private static string IsExistByPersonIdMethod() =>
                 $@"public static async Task<bool> Is{FormattedTNSingle}ExistsByPersonIdAsync({TableIdDT} personId)
        {{
            if (personId <= 0)
                throw new ArgumentException(""Person ID must be greater than zero"", nameof(personId));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Is{FormattedTNSingle}ExistsByPersonId"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@PersonId"", personId);
                    await connection.OpenAsync();

                    var returnParameter = command.Parameters.Add(""@ReturnVal"", SqlDbType.{GetSqlDbType()});
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await command.ExecuteNonQueryAsync();

                    return ({TableIdDT})returnParameter.Value == 1;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return false;
            }}
        }}

";

    private static string TopUsing() =>
$@"using {AppName}.DTO;
using Microsoft.Data.SqlClient;
using System.Data;
using Utilities;

namespace {AppName}.Da
{{
    public partial class {DataClsName}
    {{";

    private static string GetAllMethod() =>
                 $@"public static async Task<List<{DtoClsName}>> GetAll{FormattedTNPluralize}Async(int pageNumber = 1, int pageSize = 50)
        {{
            List<{DtoClsName}> {FormattedTNPluralizeVar}List = new List<{DtoClsName}>();

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_GetAll{FormattedTNPluralize}"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue(""@PageNumber"", pageNumber);
                    command.Parameters.AddWithValue(""@PageSize"", pageSize);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {{
                        while (await reader.ReadAsync())
                        {{
                            {FormattedTNPluralizeVar}List.Add(new DtoClsName(
{GetReaderAssignments()}
                            ));
                        }}
                    }}
                }}

                return {FormattedTNPluralizeVar}List;
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return new List<DtoClsName>();
            }}
        }}

";

    private static string GetByIDMethod() =>
                 $@"public static async Task<{DtoClsName}> Get{FormattedTNSingle}ByIdAsync({TableIdDT} {TableId.ToLower()})
        {{
            if ({TableId.ToLower()} == null)
            {{
                return null;
            }}

            if ({TableId.ToLower()} <= 0)
                throw new ArgumentException(""{FormattedTNSingle} ID must be greater than zero"", nameof({TableId.ToLower()}));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Get{FormattedTNSingle}ById"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@{TableId}"", {TableId.ToLower()});
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {{
                        if (await reader.ReadAsync())
                        {{
                            return new DtoClsName(
{GetReaderAssignments()}
                            );
                        }}
                        else
                        {{
                            return null;
                        }}
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return null;
            }}
        }}

";

    private static string GetByCountryNameMethod() =>
         $@"public static async Task<{DtoClsName}> Get{FormattedTNSingle}ByCountryNameAsync(string CountryName)
        {{
             if (string.IsNullOrWhiteSpace(CountryName))
            {{
                throw new ArgumentException(""Country name must be valid"", nameof(CountryName));
            }}

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Get{FormattedTNSingle}ByCountryName"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@CountryName"", CountryName);
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {{
                        if (await reader.ReadAsync())
                        {{
                            return new {DtoClsName}(
{GetReaderAssignments()}
                            );
                        }}
                        else
                        {{
                            return null;
                        }}
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return null;
            }}
        }}

";

    private static string GetByUsernameAndPasswordMethod() =>
                 $@"public static async Task<{DtoClsName}> Get{FormattedTNSingle}ByUsernameAndPasswordAsync(string username, string password)
        {{
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(""Username cannot be empty"", nameof(username));
                
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(""Password cannot be empty"", nameof(password));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Get{FormattedTNSingle}ByUsernameAndPassword"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@Username"", username);
                    command.Parameters.AddWithValue(""@Password"", SecurityHelper.HashPassword(password));
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {{
                        if (await reader.ReadAsync())
                        {{
                            return new {DtoClsName}(
{GetReaderAssignments()}
                            );
                        }}
                        else
                        {{
                            return null;
                        }}
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return null;
            }}
        }}

";

    private static string GetByPersonIDMethod() =>
                 $@"public static async Task<{DtoClsName}> Get{FormattedTNSingle}ByPersonIdAsync({TableIdDT} personId)
        {{
            if (personId <= 0)
                throw new ArgumentException(""Person ID must be greater than zero"", nameof(personId));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Get{FormattedTNSingle}ByPersonId"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@PersonId"", personId);
                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {{
                        if (await reader.ReadAsync())
                        {{
                            return new {DtoClsName}(
{GetReaderAssignments()}
                            );
                        }}
                        else
                        {{
                            return null;
                        }}
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return null;
            }}
        }}

";

    private static string AddNewMethod() =>
                 $@"public static async Task<{TableIdDT}> Add{FormattedTNSingle}Async({DtoClsName} {FormattedTNSingleVar.ToLower()}DTO)
        {{
            if ({FormattedTNSingleVar.ToLower()}DTO == null)
            {{
                throw new ArgumentNullException(nameof({FormattedTNSingleVar.ToLower()}DTO));
            }}

            if (!_Validate{FormattedTNSingle}({FormattedTNSingleVar.ToLower()}DTO))
            {{
                return -1;
            }}

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Add{FormattedTNSingle}"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;

{GetCommandParametersForAddUpdate()}

                    SqlParameter outputIdParam = new SqlParameter(""@New{TableId}"", SqlDbType.{GetSqlDbType()})
                    {{
                        Direction = ParameterDirection.Output
                    }};
                    command.Parameters.Add(outputIdParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    return ({TableIdDT})outputIdParam.Value;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return -1;
            }}
        }}

";

    private static string UpdateMethod() =>
                 $@"public static async Task<bool> Update{FormattedTNSingle}Async({DtoClsName} {FormattedTNSingleVar.ToLower()}DTO)
        {{
            if ({FormattedTNSingleVar.ToLower()}DTO == null)
            {{
                throw new ArgumentNullException(nameof({FormattedTNSingleVar.ToLower()}DTO));
            }}

            if ({FormattedTNSingleVar.ToLower()}DTO.Id <= 0)
            {{
                throw new ArgumentException(""{FormattedTNSingle} ID must be greater than zero"", nameof({FormattedTNSingleVar.ToLower()}DTO.Id));
            }}

            _Validate{FormattedTNSingle}({FormattedTNSingleVar.ToLower()}DTO);

            try
            {{
                using (var connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (var command = new SqlCommand(""SP_Update{FormattedTNSingle}"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue(""@{TableId}"", {FormattedTNSingleVar.ToLower()}DTO.Id);
{GetCommandParametersForAddUpdate()}

                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync() == 1;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return false;
            }}
        }}

";

    private static string DeleteMethod() =>
                 $@"public static async Task<bool> Delete{FormattedTNSingle}Async({TableIdDT} {TableId.ToLower()})
        {{
            if ({TableId.ToLower()} <= 0)
                throw new ArgumentException(""{FormattedTNSingle} ID must be greater than zero"", nameof({TableId.ToLower()}));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Delete{FormattedTNSingle}"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@{TableId}"", {TableId.ToLower()});

                    await connection.OpenAsync();
                    {TableIdDT} rowsAffected = ({TableIdDT})await command.ExecuteScalarAsync();

                    return rowsAffected == 1;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return false;
            }}
        }}

";

    private static string IsExistMethod() =>
                 $@"public static async Task<bool> Is{FormattedTNSingle}ExistsAsync({TableIdDT} {TableId.ToLower()})
        {{
            if ({TableId.ToLower()} <= 0)
                throw new ArgumentException(""{FormattedTNSingle} ID must be greater than zero"", nameof({TableId.ToLower()}));

            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_Is{FormattedTNSingle}Exists"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue(""@{TableId}"", {TableId.ToLower()});
                    await connection.OpenAsync();

                    var returnParameter = command.Parameters.Add(""@ReturnVal"", SqlDbType.{GetSqlDbType()});
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await command.ExecuteNonQueryAsync();

                    return ({TableIdDT})returnParameter.Value == 1;
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return false;
            }}
        }}

";

    private static string CountMethod() =>
         $@"public static async Task<{TableIdDT}> {FormattedTNPluralize}{MethodNames.Count}()
        {{
            try
            {{
                using (SqlConnection connection = new SqlConnection(DataAccessSettings.ConnectionString()))
                using (SqlCommand command = new SqlCommand(""SP_{FormattedTNPluralize}Count"", connection))
                {{
                    command.CommandType = CommandType.StoredProcedure;
                    await connection.OpenAsync();

                    object result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {{
                        return Convert.ToInt32(result);
                    }}
                    else
                    {{
                        return 0;
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                Helper.ErrorLogger(ex);
                return 0;
            }}
        }}

";

    private static string ValidationMethod()
    {
        var sb = new StringBuilder();
        sb.AppendLine($@"private static bool _Validate{FormattedTNSingle}(DtoClsName {FormattedTNSingleVar.ToLower()})
        {{");

        // Add validation logic for required fields
        foreach (var column in Columns)
        {
            if (!column.IsNullable && !column.IsPrimaryKey && column.Name.ToLower() != TableId.ToLower())
            {
                string propertyName = FormatHelper.CapitalizeFirstChars(FormatId(column.Name));
                string csharpType = Helper.GetCSharpType(column.DataType);

                if (csharpType == "string")
                {
                    sb.AppendLine($@"            if (string.IsNullOrWhiteSpace({FormattedTNSingleVar.ToLower()}.{propertyName}))
            {{
                throw new ArgumentException(""{propertyName} is required"", nameof({FormattedTNSingleVar.ToLower()}.{propertyName}));
            }}");
                }
                else if (csharpType == "DateTime")
                {
                    sb.AppendLine($@"            if ({FormattedTNSingleVar.ToLower()}.{propertyName} > DateTime.Now)
            {{
                throw new ArgumentException(""{propertyName} cannot be in the future"", nameof({FormattedTNSingleVar.ToLower()}.{propertyName}));
            }}

            if ({FormattedTNSingleVar.ToLower()}.{propertyName} < DateTime.Now.AddYears(-150))
            {{
                throw new ArgumentException(""{propertyName} is too far in the past"", nameof({FormattedTNSingleVar.ToLower()}.{propertyName}));
            }}");
                }
                else if (csharpType == "int" || csharpType == "long" || csharpType == "decimal" || csharpType == "double" || csharpType == "float")
                {
                    sb.AppendLine($@"            if ({FormattedTNSingleVar.ToLower()}.{propertyName} <= 0)
            {{
                throw new ArgumentException(""{propertyName} must be greater than zero"", nameof({FormattedTNSingleVar.ToLower()}.{propertyName}));
            }}");
                }
            }
        }

        sb.AppendLine(@"
            return true;
        }");

        return sb.ToString();
    }

    private static string Closing()
    {
        return $@"    }}
}}";
    }

    #endregion

    #region  Support Methods

    private static string GetSqlDbType() => TableIdDT == "long" ? "BigInt" : "Int";

    private static string GetReaderAssignments()
    {
        var sb = new StringBuilder();
        bool firstLine = true;

        foreach (var column in Columns)
        {
            if (!firstLine)
            {
                sb.AppendLine(",");
            }
            else
            {
                firstLine = false;
            }

            sb.Append($"                                {GetReaderAssignment(column.Name, Helper.GetCSharpType(column.DataType), column.IsNullable)}");
        }

        return sb.ToString();
    }

    private static string GetReaderAssignment(string columnName, string csharpType, bool isNullable)
    {
        return $"reader.IsDBNull(reader.GetOrdinal(\"{columnName}\")) ? {Helper.GetDefaultValue(csharpType, isNullable)} : {GetReaderMethod(csharpType)}(reader.GetOrdinal(\"{columnName}\"))";
    }

    private static string GetReaderMethod(string csharpType)
    {
        switch (csharpType)
        {
            case "string": return "reader.GetString";
            case "int": return "reader.GetInt32";
            case "decimal": return "reader.GetDecimal";
            case "double": return "reader.GetDouble";
            case "float": return "reader.GetFloat";
            case "DateTime": return "reader.GetDateTime";
            case "bool": return "reader.GetBoolean";
            case "byte[]": return "reader.GetBytes";
            case "byte": return "reader.GetByte";
            case "short": return "reader.GetInt16";
            case "long": return "reader.GetInt64";
            default: return "reader.GetValue";
        }
    }

    private static string GetCommandParametersForAddUpdate()
    {
        var sb = new StringBuilder();

        foreach (var column in Columns)
        {
            if (column.IsPrimaryKey)
            {
                continue;
            }

            string propertyName = FormatHelper.CapitalizeFirstChars(FormatId(column.Name));

            if (column.IsNullable)
            {
                sb.AppendLine($@"                    command.Parameters.AddWithValue(""@{propertyName}"", {FormattedTNSingleVar.ToLower()}DTO.{propertyName} ?? (object)DBNull.Value);");
            }
            else
            {
                sb.AppendLine($@"                    command.Parameters.AddWithValue(""@{propertyName}"", {FormattedTNSingleVar.ToLower()}DTO.{propertyName});");
            }
        }

        return sb.ToString();
    }

    #endregion

    public bool GenerateAdoDaCode(out string? filePath)
    {
        filePath = null;

        StringBuilder dalCode = new StringBuilder();

        dalCode.Append(TopUsing());
        dalCode.Append(GetByIDMethod());

        if (FormatHelper.Singularize(TableName.ToLower()) == "user")
        {
            dalCode.Append(GetByUsernameAndPasswordMethod());
            dalCode.Append(GetByPersonIDMethod());
        }

        if (FormatHelper.Singularize(TableName.ToLower()) == "country")
        {
            dalCode.Append(GetByCountryNameMethod());
        }

        dalCode.Append(AddNewMethod());
        dalCode.Append(UpdateMethod());
        dalCode.Append(GetAllMethod());
        dalCode.Append(IsExistMethod());

        if (FormatHelper.Singularize(TableName.ToLower()) == "user")
        {
            dalCode.Append(IsExistByUsernameMethod());
            dalCode.Append(IsExistByPersonIdMethod());
        }

        dalCode.Append(CountMethod());
        dalCode.Append(DeleteMethod());
        dalCode.Append(ValidationMethod());
        dalCode.Append(Closing());

        string folderPath = Path.Combine(StoringPath, "DataAccess");
        string fileName = $"{DataClsName}.cs";

        bool success = FileHelper.StoreToFile(dalCode.ToString(), fileName, folderPath, true) && new ClsSpGen(TableName).GenerateAllSPs();

        if (success)
        {
            filePath = Path.Combine(folderPath, fileName);
        }

        return success;
    }

}