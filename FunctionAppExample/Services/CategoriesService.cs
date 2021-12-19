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

    public async Task<DeleteCategoryResult> DeleteCategoryAsync(string categoryId, string userId)
    {
        var result = await CategoriesRepository.DeleteCategoryAsync(categoryId, userId);
        if (result == DeleteCategoryResult.NotFound) return DeleteCategoryResult.NotFound;

        return DeleteCategoryResult.Success;
    }

    public async Task<UpdateCategoryResult> UpdateCategoryAsync(string categoryId, string userId, string name)
    {
        var categoryDocument = await CategoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null) return UpdateCategoryResult.NotFound;

        categoryDocument.Name = name;
        await CategoriesRepository.UpdateCategoryAsync(categoryDocument);

        return UpdateCategoryResult.Success;
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

    public Task<CategorySummariesResponse> ListCategoriesAsync(string userId)
    {
        return CategoriesRepository.ListCategoriesAsync(userId);
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