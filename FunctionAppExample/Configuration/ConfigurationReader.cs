using System;

namespace FunctionAppExample.Configuration;

public class ConfigurationReader : IConfigurationReader
{
    public ConfigurationReader()
    {
        var endpointUrl = Environment.GetEnvironmentVariable("CosmosDBAccountEndpointUrl");
        var accountKey = Environment.GetEnvironmentVariable("CosmosDBAccountKey");
        var databaseName = Environment.GetEnvironmentVariable("DatabaseName");
        var collectionName = Environment.GetEnvironmentVariable("CollectionName");

        CosmosDb = new CosmosDbConfiguration(collectionName, databaseName, accountKey, endpointUrl);

        var searchApiEndpoint = Environment.GetEnvironmentVariable("CognitiveServicesSearchApiEndpoint");
        var searchApiKey = Environment.GetEnvironmentVariable("CognitiveServicesSearchApiKey");

        CognitiveServices = new CognitiveServicesConfiguration(searchApiEndpoint, searchApiKey);
    }
    
    public CosmosDbConfiguration CosmosDb { get; }
    public CognitiveServicesConfiguration CognitiveServices { get; }
}

public class CosmosDbConfiguration
{
    public CosmosDbConfiguration(string collectionName, string databaseName, string accountKey, string endpointUrl)
    {
        CollectionName = collectionName;
        DatabaseName = databaseName;
        AccountKey = accountKey;
        EndpointUrl = endpointUrl;
    }

    public string CollectionName { get; }
    public string DatabaseName { get; }
    public string AccountKey { get; }
    public string EndpointUrl { get; }
}

public class CognitiveServicesConfiguration
{
    public CognitiveServicesConfiguration(string searchApiKey, string searchApiEndpoint)
    {
        SearchApiKey = searchApiKey;
        SearchApiEndpoint = searchApiEndpoint;
    }

    public string SearchApiKey { get; }

    public string SearchApiEndpoint { get; }
}