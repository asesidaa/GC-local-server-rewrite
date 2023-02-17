using System.Text.Json.Serialization;
using WebUI.Common.Models;

namespace WebUI.Common.SerializerContexts;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(List<Avatar>))]
[JsonSerializable(typeof(List<Navigator>))]
[JsonSerializable(typeof(List<Title>))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
    
}