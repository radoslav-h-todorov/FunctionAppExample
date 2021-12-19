using System.Linq;
using System.Threading.Tasks;
using FunctionAppExample.Models;
using FunctionAppExample.Repositories;
using FunctionAppExample.ResponseDtos;

namespace FunctionAppExample.Services;

public class CategoriesService : ICategoriesService
{
    protected ICategoriesRepository CategoriesRepository;
    protected IImageSearchService ImageSearchService;

    public CategoriesService(ICategoriesRepository categoriesRepository, IImageSearchService imageSearchService)
    {
        CategoriesRepository = categoriesRepository;
        ImageSearchService = imageSearchService;
    }

    public async Task<string> AddCategoryAsync(string name, string userId)
    {
        var categoryDocument = new CategoryDocument
        {
            Name = name,
            UserId = userId
        };
        var categoryId = await CategoriesRepository.AddCategoryAsync(categoryDocument);

        return categoryId;
    }

    public async Task<bool> DeleteCategoryAsync(string categoryId, string userId)
    {
        return await CategoriesRepository.DeleteCategoryAsync(categoryId, userId);
    }

    public async Task<bool> UpdateCategoryAsync(string categoryId, string userId, string name)
    {
        var categoryDocument = await CategoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null) return false;

        categoryDocument.Name = name;
        await CategoriesRepository.UpdateCategoryAsync(categoryDocument);

        return true;
    }

    public async Task<CategoryDetailsResponse> GetCategoryAsync(string categoryId, string userId)
    {
        var categoryDocument = await CategoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null) return null;

        return new CategoryDetailsResponse
        {
            Id = categoryDocument.Id,
            ImageUrl = categoryDocument.ImageUrl,
            Name = categoryDocument.Name,
            Synonyms = categoryDocument.Synonyms,
            Items = categoryDocument.Items.Select(i => new CategoryItemDetailsResponse
            {
                Id = i.Id,
                Type = i.Type,
                Preview = i.Preview
            }).ToList()
        };
    }

    public async Task<CategorySummariesResponse> ListCategoriesAsync(string userId)
    {
        var categoryDocuments = await CategoriesRepository.ListCategoriesAsync(userId);
        var categorySummaries = new CategorySummariesResponse();
        
        foreach (var categoryDocument in categoryDocuments)
        {
            categorySummaries.Add(new CategorySummaryResponse(){ Id = categoryDocument.Id, Name = categoryDocument.Name});
        }

        return categorySummaries;
    }

    public async Task<bool> UpdateCategoryImageAsync(string categoryId, string userId)
    {
        var categoryDocument = await CategoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null) return false;

        var imageUrl = await ImageSearchService.FindImageUrlAsync(categoryDocument.Name);
        if (string.IsNullOrEmpty(imageUrl)) return false;

        // get the document again, to reduce the likelihood of concurrency races
        categoryDocument = await CategoriesRepository.GetCategoryAsync(categoryId, userId);

        categoryDocument.ImageUrl = imageUrl;
        await CategoriesRepository.UpdateCategoryAsync(categoryDocument);

        return true;
    }
}