using System.Text;
using Utils.FileActions;
using Utils.Format;
using Utils.General;

namespace Bl.Gen;

public class ClsDtoGen : ClsBaseGen
{
    public ClsDtoGen(string tableName) : base(tableName) { }

    private static string Properties()
    {
        var sb = new StringBuilder();

        foreach (var column in Columns)
        {
            string csharpType = Helper.GetCSharpType(column.DataType);
            string nullableSymbol = column.IsNullable ? "?" : "";
            string propertyName = FormatHelper.CapitalizeFirstChars(FormatId(column.Name));

            if (column.IsPrimaryKey)
            {
                sb.AppendLine($"        public {csharpType}{nullableSymbol} Id");
            }
            else
            {
                sb.AppendLine($"        public {csharpType}{nullableSymbol} {propertyName}");
            }

            sb.AppendLine("        {");
            sb.AppendLine("            get;");
            sb.AppendLine("            set;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public bool GenerateDTO(out string filePath)
    {
        filePath = null;

        string TopUsing = $@"namespace Bl.Dtos.Business;

    public class {DtoClsName}
    {{";

        var dto = new StringBuilder();
        dto.AppendLine(TopUsing);

        dto.AppendLine(Properties());
        dto.Append($@"}}");

        string folderPath = Path.Combine(StoringPath, "DTO");
        string fileName = $"{DtoClsName}.cs";

        bool success = FileHelper.StoreToFile(dto.ToString(), fileName, folderPath, true);

        if (success)
        {
            filePath = Path.Combine(folderPath, fileName);
        }

        return success;
    }

}