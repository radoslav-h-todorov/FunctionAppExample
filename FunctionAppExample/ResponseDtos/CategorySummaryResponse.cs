using Newtonsoft.Json;

namespace FunctionAppExample.ResponseDtos;

public class CategorySummaryResponse
{
    public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }
}