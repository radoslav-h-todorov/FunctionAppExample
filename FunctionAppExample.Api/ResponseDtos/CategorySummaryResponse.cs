using Newtonsoft.Json;

namespace FunctionAppExample.Api.ResponseDtos;

public class CategorySummaryResponse
{
    public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }
}