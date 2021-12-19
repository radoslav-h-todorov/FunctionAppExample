using System.Web;
using Newtonsoft.Json.Linq;

namespace FunctionAppExample.BusinessLogic.ImageSearch;

public class ImageSearchService : IImageSearchService
{
    private readonly string _searchApiEndpoint = Environment.GetEnvironmentVariable("CognitiveServicesSearchApiEndpoint");
    private readonly string _searchApiKey = Environment.GetEnvironmentVariable("CognitiveServicesSearchApiKey");
    
    private readonly HttpClient _httpClient;
    private readonly Random _random;

    public ImageSearchService(Random random, HttpClient httpClient)
    {
        _random = random;
        _httpClient = httpClient;
    }

    public async Task<string> FindImageUrlAsync(string searchTerm)
    {
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _searchApiKey);

        // construct the URI of the search request
        var uriBuilder = new UriBuilder(_searchApiEndpoint);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["q"] = searchTerm;
        uriBuilder.Query = query.ToString();
        var uriQuery = uriBuilder.ToString();

        // execute the request
        var response = await _httpClient.GetAsync(uriQuery);
        response.EnsureSuccessStatusCode();

        // get the results
        var contentString = await response.Content.ReadAsStringAsync();
        dynamic responseJson = JObject.Parse(contentString);
        var results = (JArray) responseJson.value;
        if (results.Count == 0)
        {
            return null;
        }

        // pick a random result
        var index = _random.Next(0, results.Count - 1);
        var topResult = (dynamic) results[index];
        
        return topResult.contentUrl;
    }
}