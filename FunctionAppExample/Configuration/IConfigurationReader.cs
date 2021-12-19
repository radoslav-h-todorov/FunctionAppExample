namespace FunctionAppExample.Configuration;

public interface IConfigurationReader
{
    CosmosDbConfiguration CosmosDb { get; }

    CognitiveServicesConfiguration CognitiveServices { get; }
}