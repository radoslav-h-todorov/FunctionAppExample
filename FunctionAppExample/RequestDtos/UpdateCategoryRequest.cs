using Newtonsoft.Json;

namespace FunctionAppExample.RequestDtos;

public class UpdateCategoryRequest
{
    [JsonProperty("id")]
    public string Id;

    [JsonProperty("name")]
    public string Name;
}