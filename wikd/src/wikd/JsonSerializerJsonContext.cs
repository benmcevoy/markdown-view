namespace wikd;

// JsonSerializerJsonContext is source generated, requires a build to create the implementation
// can then use JsonSerializerJsonContext.Default.MyType
// This class cannot be nested inside SearchService as that 
// prevents SearchService from building... chicken and egg... circular dependancy
using System.Text.Json;
using System.Text.Json.Serialization;


[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(http.Response))]
[JsonSerializable(typeof(SearchResult))]
[JsonSerializable(typeof(SearchResultItem))]
internal partial class JsonSerializerJsonContext : JsonSerializerContext { }
