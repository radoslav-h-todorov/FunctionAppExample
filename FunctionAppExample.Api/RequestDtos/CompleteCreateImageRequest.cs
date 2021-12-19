using Newtonsoft.Json;

namespace FunctionAppExample.Api.RequestDtos;

public class CompleteCreateImageRequest
{
    [JsonProperty("categoryId")]
    public string CategoryId { get; set; }
}