using Newtonsoft.Json;

namespace FunctionAppExample.RequestDtos;

public class CreateCategoryRequest
{
    [JsonProperty("name")]
    public string Name;
}