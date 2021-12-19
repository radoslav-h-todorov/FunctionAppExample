using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FunctionAppExample.Configuration;
using Newtonsoft.Json.Linq;

namespace FunctionAppExample.Services;

public class ImageSearchService : IImageSearchService
{
    private readonly IConfigurationReader _configurationReader;
    private readonly HttpClient _httpClient;
    private readonly Random _random;

    public ImageSearchService(Random random, HttpClient httpClient, IConfigurationReader configurationReader)
    {
        _random = random;
        _httpClient = httpClient;
        _configurationReader = configurationReader;
    }

    public async Task<string> FindImageUrlAsync(string searchTerm)
    {
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _configurationReader.CognitiveServices.SearchApiKey);

        // construct the URI of the search request
        var uriBuilder = new UriBuilder(_configurationReader.CognitiveServices.SearchApiEndpoint);
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
        if (results.Count == 0) return null;

        // pick a random result
        var index = _random.Next(0, results.Count - 1);
        var topResult = (dynamic) results[index];
        return topResult.contentUrl;
    }
}