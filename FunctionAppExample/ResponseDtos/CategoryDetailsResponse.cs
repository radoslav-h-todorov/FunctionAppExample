using System.Collections.Generic;
using Newtonsoft.Json;

namespace FunctionAppExample.ResponseDtos;

public class CategoryDetailsResponse
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("imageUrl")] public string ImageUrl { get; set; }
    
    [JsonProperty("items")] public IList<CategoryItemDetailsResponse> Items { get; set; }
}