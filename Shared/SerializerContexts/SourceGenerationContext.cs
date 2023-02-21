using System.Text.Json.Serialization;
using Shared.Dto.Api;
using Shared.Models;

namespace Shared.SerializerContexts;

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ServiceResult<List<ClientCardDto>>))]
[JsonSerializable(typeof(ServiceResult<PlayOptionData>))]
[JsonSerializable(typeof(ServiceResult<bool>))]
[JsonSerializable(typeof(ServiceResult<TotalResultData>))]
[JsonSerializable(typeof(ServiceResult<TotalResultData>))]
public partial class SourceGenerationContext : JsonSerializerContext
{
    
}