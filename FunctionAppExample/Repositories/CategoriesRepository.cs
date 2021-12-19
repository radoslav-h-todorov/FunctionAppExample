using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FunctionAppExample.Configuration;
using FunctionAppExample.Models;
using FunctionAppExample.ResponseDtos;
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

    public async Task<DeleteCategoryResult> DeleteCategoryAsync(string categoryId, string userId)
    {
        var documentUri = UriFactory.CreateDocumentUri(_configurationReader.CosmosDb.DatabaseName,
            _configurationReader.CosmosDb.CollectionName, categoryId);
        try
        {
            await _documentClient.DeleteDocumentAsync(documentUri,
                new RequestOptions {PartitionKey = new PartitionKey(userId)});
            return DeleteCategoryResult.Success;
        }
        catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // we return the NotFound result to indicate the document was not found
            return DeleteCategoryResult.NotFound;
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
            // we return null to indicate the document was not found
            return null;
        }
    }

    public async Task<CategoryDocument> FindCategoryWithItemAsync(string itemId, ItemType itemType, string userId)
    {
        var documentUri =
            UriFactory.CreateDocumentCollectionUri(_configurationReader.CosmosDb.DatabaseName,
                _configurationReader.CosmosDb.CollectionName);

        // create a query to find the category with this item in it
        var sqlQuery =
            "SELECT * FROM c WHERE c.userId = @userId AND ARRAY_CONTAINS(c.items, { id: @itemId, type: @itemType }, true)";
        var sqlParameters = new SqlParameterCollection
        {
            new("@userId", userId),
            new("@itemId", itemId),
            new("@itemType", itemType.ToString())
        };
        var query = _documentClient
            .CreateDocumentQuery<CategoryDocument>(documentUri, new SqlQuerySpec(sqlQuery, sqlParameters))
            .AsDocumentQuery();

        // execute the query
        var response = await query.ExecuteNextAsync<CategoryDocument>();
        return response.SingleOrDefault();
    }

    public async Task<CategorySummaries> ListCategoriesAsync(string userId)
    {
        var documentUri =
            UriFactory.CreateDocumentCollectionUri(_configurationReader.CosmosDb.DatabaseName,
                _configurationReader.CosmosDb.CollectionName);

        // create a query to just get the document ids
        var query = _documentClient
            .CreateDocumentQuery<CategoryDocument>(documentUri)
            .Where(d => d.UserId == userId)
            .Select(d => new CategorySummary {Id = d.Id, Name = d.Name})
            .AsDocumentQuery();

        // iterate until we have all of the ids
        var list = new CategorySummaries();
        while (query.HasMoreResults)
        {
            var summaries = await query.ExecuteNextAsync<CategorySummary>();
            list.AddRange(summaries);
        }

        return list;
    }
}