using Utils.FileActions;

namespace Bl.Gen;

public class ClsGen : ClsBaseGen
{
    public ClsGen(string tableName) : base(tableName)   {   }
   
    public bool GenerateMappingCode() => FileHelper.StoreToFile(@$"CreateMap<{ModelName}, {DtoClsName}>().ReverseMap();{Environment.NewLine}", MappingTxt, StoringPath, false);

    public bool GenerateDICode() => FileHelper.StoreToFile(@$"builder.Services.AddScoped<{LogicInterfaceName}, {ServiceClsName}>();{Environment.NewLine}", BlDiTxt, StoringPath, false);

    public bool GenerateContractsCode(out string filePath)
    {
        filePath = null;

        string folderPath = Path.Combine(StoringPath, "BlInterfaces");

        string fileContent = $@"using Bl.Contracts.Base;
using Bl.Dtos.Business;
using Domains.Models;

namespace Bl.Contracts.Business;

public interface {LogicInterfaceName} : IBaseService<{ModelName}, {DtoClsName}>
{{
  
}}";

        string interfaceName = LogicInterfaceName;
        string fileName = $"{interfaceName}.cs";
        bool success = FileHelper.StoreToFile(fileContent, fileName, folderPath, true);

        if (success)
        {
            filePath = Path.Combine(folderPath, fileName);
        }

        return success;
    }

    public bool GenerateBlCode(out string filePath)
    {
        filePath = null;

        string code = @$"using AutoMapper;
using Bl.Contracts.Auth;
using Bl.Contracts.Business;
using Bl.Dtos.Business;
using Bl.Services.Base;
using Da.Contracts;
using Bl.Contracts.Events;
using Domains.Models;

namespace Bl.Services.Business;

public class {ServiceClsName} : BaseService<{ModelName}, {DtoClsName}>, {LogicInterfaceName}
{{
    public {ServiceClsName}(ITableRepo<{ModelName}> repo, IMapper mapper, IUserServiceQuery userServiceQuery,IEntityChangePublisher eventPublisher) : base(repo, mapper, userServiceQuery, eventPublisher)
    {{

    }}

}}";

        string folderPath = Path.Combine(StoringPath, "Logic");
        string fileName = $"{ServiceClsName}.cs";

        bool success = FileHelper.StoreToFile(code, fileName, folderPath, true);

        if (success)
        {
            filePath = Path.Combine(folderPath, fileName);
        }

        return success;
    }

}
