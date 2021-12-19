using Newtonsoft.Json;

namespace FunctionAppExample.Api.RequestDtos;

public class CreateCategoryRequest
{
    [JsonProperty("name")] public string Name;
}