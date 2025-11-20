using System.Text;
using Utils.FileActions;

namespace Bl.Gen;

public class ClsAPIGen : ClsBaseGen
{
    private static int _versionNumber = 1;
    private static readonly object _versionLock = new object();

    public ClsAPIGen(string tableName) : base(tableName) { }

    /// <summary>
    /// Gets or sets the application version number.
    /// </summary>
    /// <value>The current version number of the application.</value>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when setting a negative version number.</exception>
    public static int VersionNumber
    {
        get
        {
            lock (_versionLock)
            {
                return _versionNumber;
            }
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Version number cannot be negative.");
            }

            lock (_versionLock)
            {
                _versionNumber = value;
            }
        }
    }

    private static string ControllerName
    {
        get
        {
            return $"{FormattedTNPluralize}Controller";
        }
    }

    #region Class Structure

    private static string TopUsing()
    {
        return $@"using Microsoft.AspNetCore.Mvc;
using Bl.Contracts.Business;
using Microsoft.AspNetCore.Authorization;
using Bl.Dtos.Business;
using Api.Models;
using Da.Models;
using System.Text.Json;

namespace Api.Controllers.Business;

[Route(""api/v{VersionNumber}/{FormattedTNPluralize}"")]
[ApiController]
[Authorize]
public class {ControllerName} : ControllerBase
{{";
    }

    private static string Constructor()
    {
        return @$"        {LogicInterfaceName} {LogicObjName};
        
        public {ControllerName}({LogicInterfaceName} {FormattedTNSingleVar})
        {{
            {LogicObjName} = {FormattedTNSingleVar};
            
        }}";
    }

    private static string GetByIdEndpoint() =>
                     @$"    [HttpGet(""ById/{{Id:Guid:min(1)}}"", Name = ""Get{FormattedTNSingle}ById"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<{DtoClsName}>>> Get{FormattedTNSingle}ById(Guid Id)
    {{
        try
        {{
            var {FormattedTNSingleVar} = await {LogicObjName}.{MethodNames.GetById}(Id);

            if ({FormattedTNSingleVar} == null)
            {{
                return NotFound(ApiResponse<{DtoClsName}>.FailResponse($""{FormattedTNSingle} with ID {{Id}} not found.""));
            }}

            return Ok(ApiResponse<{DtoClsName}>.SuccessResponse({FormattedTNSingleVar}));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<{DtoClsName}>.FailResponse(""An error occurred while fetching the address"",
                new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string CreateEndpoint() =>
                 @$"    [HttpPost(Name = ""Add{FormattedTNSingle}"")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<{DtoClsName}>>> Add{FormattedTNSingle}({DtoClsName} new{FormattedTNSingle})
    {{
        try
        {{
            if (!ModelState.IsValid)
            {{
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<{DtoClsName}>.FailResponse(""Invalid {FormattedTNSingleVar} data"", errors));
            }}

            var addResult = await {LogicObjName}.{MethodNames.Add}(new{FormattedTNSingle});

            var {FormattedTNSingleVar}Id = addResult.id;

            if ({FormattedTNSingleVar}Id == Guid.Empty || !addResult.success)
            {{
                return BadRequest(ApiResponse<{DtoClsName}>.FailResponse(""Failed to add {FormattedTNSingleVar}""));
            }}

            new{FormattedTNSingle}.Id = {FormattedTNSingleVar}Id;

            return CreatedAtRoute(
                ""Get{FormattedTNSingle}ById"",
                new {{ id = {FormattedTNSingleVar}Id }},
                ApiResponse<{DtoClsName}>.SuccessResponse(new{FormattedTNSingle}, ""{FormattedTNSingle} created successfully""));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<{DtoClsName}>.FailResponse(""An error occurred while adding the {FormattedTNSingleVar}"",
                new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string UpdateEndpoint() =>
                     @$"    [HttpPut(""{{Id:Guid:min(1)}}"", Name = ""Update{FormattedTNSingle}"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<{DtoClsName}>>> Update{FormattedTNSingle}(Guid Id, {DtoClsName} updated{FormattedTNSingle})
    {{
        try
        {{
            if (!ModelState.IsValid)
            {{
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<{DtoClsName}>.FailResponse(""Invalid {FormattedTNSingleVar} data"", errors));
            }}

            if (!await {LogicObjName}.{MethodNames.IsExists}(Id))
            {{
                return NotFound(ApiResponse<{DtoClsName}>.FailResponse($""{FormattedTNSingle} with ID {{Id}} not found""));
            }}

            updated{FormattedTNSingle}.Id = Id;

            var updateResult = await {LogicObjName}.{MethodNames.Update}(updated{FormattedTNSingle});
            if (!updateResult)
            {{
                return BadRequest(ApiResponse<{DtoClsName}>.FailResponse(""Failed to update {FormattedTNSingleVar}""));
            }}

            return Ok(ApiResponse<{DtoClsName}>.SuccessResponse(updated{FormattedTNSingle}, ""{FormattedTNSingle} updated successfully""));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<{DtoClsName}>.FailResponse(""An error occurred while updating the {FormattedTNSingleVar}"",
                new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string GetAllEndpoint() =>
                     @$"    [HttpGet(""All"", Name = ""GetAll{FormattedTNPluralize}"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<AddressDto>>>> GetAll{FormattedTNPluralize}(int pageNumber = 1, int pageSize = 50)
    {{
        try
        {{
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {{
                return BadRequest(ApiResponse<PagedResult<{DtoClsName}>>.FailResponse(
                    ""Invalid pagination parameters. Page number must be ≥ 1 and page size between 1-100.""));
            }}

            var {FormattedTNPluralizeVar}List = await {LogicObjName}.{MethodNames.GetPagedList}<{DtoClsName}>(pageNumber, pageSize);

            if ({FormattedTNPluralizeVar}List.TotalCount == 0)
            {{
                return NotFound(ApiResponse<PagedResult<{DtoClsName}>>.FailResponse(""No {FormattedTNPluralizeVar} found""));
            }}

            // Add pagination metadata to response headers
            Response.Headers.Append(""X-Pagination"",
                JsonSerializer.Serialize(new
                {{
                    {FormattedTNPluralizeVar}List.TotalCount,
                    {FormattedTNPluralizeVar}List.PageSize,
                    {FormattedTNPluralizeVar}List.CurrentPage,
                    {FormattedTNPluralizeVar}List.TotalPages,
                    {FormattedTNPluralizeVar}List.HasNext,
                    {FormattedTNPluralizeVar}List.HasPrevious
                }}));

            return Ok(ApiResponse<PagedResult<{DtoClsName}>>.SuccessResponse(
                {FormattedTNPluralizeVar}List,
                $""Found {{{FormattedTNPluralizeVar}List.TotalCount}} {FormattedTNPluralizeVar}""));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<PagedResult<{DtoClsName}>>.FailResponse(
                    ""An error occurred while retrieving {FormattedTNPluralizeVar}"",
                    new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string IsExistsEndpoint() =>
         $@"    [HttpGet(""IsExists/{{Id:Guid:min(1)}}"", Name = ""Is{FormattedTNSingle}Exists"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> Is{FormattedTNSingle}Exists(Guid Id)
    {{
        try
        {{
            bool exists = await {LogicObjName}.{MethodNames.IsExists}(Id);

            return exists
                ? Ok(ApiResponse<bool>.SuccessResponse(true, ""{FormattedTNSingle} exists""))
                : NotFound(ApiResponse<bool>.FailResponse(""{FormattedTNSingle} not found""));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<bool>.FailResponse(
                    ""An error occurred while checking {FormattedTNSingleVar} existence"",
                    new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string GetCountEndpoint() =>
         $@"    [HttpGet(""Count"", Name = ""Get{FormattedTNPluralize}Count"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> Get{FormattedTNPluralize}Count()
    {{
        try
        {{
            var count = await {LogicObjName}.{MethodNames.Count}();
            return Ok(ApiResponse<int>.SuccessResponse(count, $""Found {{count}} {FormattedTNPluralizeVar}""));
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<int>.FailResponse(
                    ""An error occurred while counting {FormattedTNPluralizeVar}"",
                    new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string DeleteEndpoint() =>
         $@"    [HttpDelete(""{{Id:Guid:min(1)}}"", Name = ""Delete{FormattedTNSingle}"")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete{FormattedTNSingle}(Guid Id)
    {{
        try
        {{
            bool isDeleted = await {LogicObjName}.{MethodNames.ChangeStatus}(Id, enCurrentState.Deleted);

            if (isDeleted)
            {{
                return Ok(ApiResponse<bool>.SuccessResponse(
                    true,
                    $""{FormattedTNSingle} with ID {{Id}} has been deleted""));
            }}
            else
            {{
                return NotFound(ApiResponse<bool>.FailResponse(
                    $""{FormattedTNSingle} with ID {{Id}} not found. No rows deleted""));
            }}
        }}
        catch (Exception ex)
        {{
            return StatusCode(500,
                ApiResponse<bool>.FailResponse(
                    ""An error occurred while deleting {FormattedTNSingleVar}"",
                    new List<string> {{ ex.Message }}));
        }}
    }}";

    private static string Closing() =>
         $@"    }}";

    #endregion

    public bool GenerateControllerCode(out string filePath)
    {
        filePath = null;

        StringBuilder controllerCode = new StringBuilder();

        controllerCode.Append(TopUsing());

        ClsGen gen = new ClsGen(TableName);

        if (!gen.GenerateDICode() || !gen.GenerateMappingCode())
        {
            FileHelper.ErrorLogger(new Exception("Failed to generate DI or Mapping code."));
            return false;
        }

        controllerCode.Append(Constructor());
        controllerCode.Append(GetByIdEndpoint());
        controllerCode.Append(CreateEndpoint());
        controllerCode.Append(UpdateEndpoint());
        controllerCode.Append(GetAllEndpoint());
        controllerCode.Append(IsExistsEndpoint());
        controllerCode.Append(GetCountEndpoint());
        controllerCode.Append(DeleteEndpoint());
        controllerCode.Append(Closing());

        string folderPath = Path.Combine(StoringPath, "Controllers");
        string fileName = $"{ControllerName}.cs";
        bool success = FileHelper.StoreToFile(controllerCode.ToString(), fileName, folderPath, true);

        if (success)
        {
            filePath = Path.Combine(folderPath, fileName);
        }

        return success;
    }

}