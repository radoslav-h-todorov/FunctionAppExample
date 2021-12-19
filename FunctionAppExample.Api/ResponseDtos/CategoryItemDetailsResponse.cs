using FunctionAppExample.DataAccess.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FunctionAppExample.Api.ResponseDtos;

public class CategoryItemDetailsResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ItemType Type { get; set; }

    [JsonProperty("preview")] public string Preview { get; set; }
}