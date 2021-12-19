using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FunctionAppExample.BusinessLogic.Categories;
using FunctionAppExample.BusinessLogic.ImageSearch;
using FunctionAppExample.BusinessLogic.UserAuthentication;
using FunctionAppExample.Converters;
using FunctionAppExample.DataAccess.Repositories;
using FunctionAppExample.RequestDtos;
using FunctionAppExample.ResponseDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppExample;

public class ApiFunctions
{
    private const string JsonContentType = "application/json";

    private readonly ICategoriesService _categoriesService = new CategoriesService(new CategoriesRepository(), new ImageSearchService(new Random(), new HttpClient()));
    private readonly IUserAuthenticationService _userAuthenticationService = new QueryStringUserAuthenticationService();
    private readonly ILogger<ApiFunctions> _logger;

    public ApiFunctions(ILogger<ApiFunctions> log)
    {
        _logger = log;
    }

    [FunctionName("AddCategory")]
    public async Task<IActionResult> AddCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories")]
        HttpRequest req)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        CreateCategoryRequest data;
        try
        {
            data = JsonConvert.DeserializeObject<CreateCategoryRequest>(requestBody);
        }
        catch (JsonReaderException)
        {
            return new BadRequestObjectResult(new {error = "Body should be provided in JSON format."});
        }

        if (data == null || string.IsNullOrEmpty(data.Name))
        {
            return new BadRequestObjectResult(new {error = "Missing required property 'name'."});
        }

        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var categoryId = await _categoriesService.AddCategoryAsync(data.Name, userId);
            return new OkObjectResult(new {id = categoryId});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }

    [FunctionName("DeleteCategory")]
    public async Task<IActionResult> DeleteCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{id}")]
        HttpRequest req, string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return new BadRequestObjectResult(new {error = "Missing required argument 'id'."});
        }

        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            await _categoriesService.DeleteCategoryAsync(id,
                userId); // we ignore the result of this call - whether it's Success or NotFound, we return an 'Ok' back to the client
            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }

    [FunctionName("UpdateCategory")]
    public async Task<IActionResult> UpdateCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "categories/{id}")]
        HttpRequest req, string id)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        UpdateCategoryRequest data;
        try
        {
            data = JsonConvert.DeserializeObject<UpdateCategoryRequest>(requestBody);
        }
        catch (JsonReaderException)
        {
            return new BadRequestObjectResult(new {error = "Body should be provided in JSON format."});
        }

        if (data == null)
        {
            return new BadRequestObjectResult(new {error = "Missing required property 'name'."});
        }

        if (data.Id != null && id != null && data.Id != id)
        {
            return new BadRequestObjectResult(new
                {error = "Property 'id' does not match the identifier specified in the URL path."});
        }

        if (string.IsNullOrEmpty(data.Id))
        {
            data.Id = id;
        }

        if (string.IsNullOrEmpty(data.Name))
        {
            return new BadRequestObjectResult(new {error = "Missing required property 'name'."});
        }

        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var result = await _categoriesService.UpdateCategoryAsync(data.Id, userId, data.Name);
            if (!result) return new NotFoundResult();

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }

    [FunctionName("GetCategory")]
    public async Task<IActionResult> GetCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{id}")]
        HttpRequest req, string id)
    {
        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var categoryDocument = await _categoriesService.GetCategoryAsync(id, userId);
            if (categoryDocument == null)
            {
                return new NotFoundResult();
            }
            
            // TODO: AutoMapper
            var categoryDetailsResponse =  new CategoryDetailsResponse
            {
                Id = categoryDocument.Id,
                ImageUrl = categoryDocument.ImageUrl,
                Name = categoryDocument.Name,
                Items = categoryDocument.Items.Select(i => new CategoryItemDetailsResponse()
                {
                    Id = i.Id,
                    Type = i.Type,
                    Preview = i.Preview
                }).ToList()
            };

            return new OkObjectResult(categoryDetailsResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }

    [FunctionName("ListCategories")]
    public async Task<IActionResult> ListCategories(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories")]
        HttpRequest req)
    {
        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var categoryDocuments = await _categoriesService.ListCategoriesAsync(userId);

            // TODO: AutoMapper
            var categorySummaries = new CategorySummariesResponse();
            foreach (var categoryDocument in categoryDocuments)
            {
                categorySummaries.Add(new CategorySummaryResponse {Id = categoryDocument.Id, Name = categoryDocument.Name});
            }
            
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new CategorySummariesConverter());
            var json = JsonConvert.SerializeObject(categorySummaries, settings);

            return new ContentResult
            {
                Content = json,
                ContentType = JsonContentType,
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }
}

