using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FunctionAppExample.Configuration;
using FunctionAppExample.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace FunctionAppExample.Repositories;

public class CategoriesRepository : ICategoriesRepository
{
    private readonly IConfigurationReader _configurationReader;
    private readonly DocumentClient _documentClient;

    public CategoriesRepository(IConfigurationReader configurationReader)
    {
        _configurationReader = configurationReader;
        _documentClient =
            new DocumentClient(new Uri(_configurationReader.CosmosDb.EndpointUrl), _configurationReader.CosmosDb.AccountKey);
    }

    public async Task<string> AddCategoryAsync(CategoryDocument categoryDocument)
    {
        var documentUri =
            UriFactory.CreateDocumentCollectionUri(_configurationReader.CosmosDb.DatabaseName,
                _configurationReader.CosmosDb.CollectionName);
        Document doc = await _documentClient.CreateDocumentAsync(documentUri, categoryDocument);
        return doc.Id;
    }

    public async Task<bool> DeleteCategoryAsync(string categoryId, string userId)
    {
        var documentUri = UriFactory.CreateDocumentUri(_configurationReader.CosmosDb.DatabaseName,
            _configurationReader.CosmosDb.CollectionName, categoryId);
        try
        {
            await _documentClient.DeleteDocumentAsync(documentUri,
                new RequestOptions {PartitionKey = new PartitionKey(userId)});
            return true;
        }
        catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task UpdateCategoryAsync(CategoryDocument categoryDocument)
    {
        var documentUri = UriFactory.CreateDocumentUri(_configurationReader.CosmosDb.DatabaseName,
            _configurationReader.CosmosDb.CollectionName, categoryDocument.Id);
        var concurrencyCondition = new AccessCondition
        {
            Condition = categoryDocument.ETag,
            Type = AccessConditionType.IfMatch
        };
        
        return _documentClient.ReplaceDocumentAsync(documentUri, categoryDocument,
            new RequestOptions {AccessCondition = concurrencyCondition});
    }

    public async Task<CategoryDocument> GetCategoryAsync(string categoryId, string userId)
    {
        var documentUri = UriFactory.CreateDocumentUri(_configurationReader.CosmosDb.DatabaseName,
            _configurationReader.CosmosDb.CollectionName, categoryId);
        try
        {
            var documentResponse = await _documentClient.ReadDocumentAsync<CategoryDocument>(documentUri,
                new RequestOptions {PartitionKey = new PartitionKey(userId)});
            return documentResponse.Document;
        }
        catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<CategoryDocument>> ListCategoriesAsync(string userId)
    {
        var documentUri =
            UriFactory.CreateDocumentCollectionUri(_configurationReader.CosmosDb.DatabaseName,
                _configurationReader.CosmosDb.CollectionName);
        
        var query = _documentClient
            .CreateDocumentQuery<CategoryDocument>(documentUri)
            .Where(d => d.UserId == userId)
            .AsDocumentQuery();
        
        var list = new List<CategoryDocument>();
        while (query.HasMoreResults)
        {
            var summaries = await query.ExecuteNextAsync<CategoryDocument>();
            list.AddRange(summaries);
        }

        return list;
    }
}