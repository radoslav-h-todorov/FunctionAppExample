using System;

namespace FunctionAppExample.Configuration;

public class ConfigurationReader : IConfigurationReader
{
    public ConfigurationReader()
    {
        EndpointUrl = Environment.GetEnvironmentVariable("CosmosDBAccountEndpointUrl");
        AccountKey = Environment.GetEnvironmentVariable("CosmosDBAccountKey");
        DatabaseName = Environment.GetEnvironmentVariable("DatabaseName");
        CollectionName = Environment.GetEnvironmentVariable("CollectionName");
    }

    public string CollectionName { get; }

    public string DatabaseName { get; }

    public string AccountKey { get; }

    public string EndpointUrl { get; }
}

public interface IConfigurationReader
{
    string CollectionName { get; }
    string DatabaseName { get; }
    string AccountKey { get; }
    string EndpointUrl { get; }
}