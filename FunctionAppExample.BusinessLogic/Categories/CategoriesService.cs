using FunctionAppExample.BusinessLogic.ImageSearch;
using FunctionAppExample.DataAccess.Models;
using FunctionAppExample.DataAccess.Repositories;

namespace FunctionAppExample.BusinessLogic.Categories;

public class CategoriesService : ICategoriesService
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IImageSearchService _imageSearchService;

    public CategoriesService(ICategoriesRepository categoriesRepository, IImageSearchService imageSearchService)
    {
        _categoriesRepository = categoriesRepository;
        _imageSearchService = imageSearchService;
    }

    public async Task<string> AddCategoryAsync(string name, string userId)
    {
        var categoryDocument = new CategoryDocument
        {
            Name = name,
            UserId = userId
        };
        var categoryId = await _categoriesRepository.AddCategoryAsync(categoryDocument);

        return categoryId;
    }

    public async Task<bool> DeleteCategoryAsync(string categoryId, string userId)
    {
        return await _categoriesRepository.DeleteCategoryAsync(categoryId, userId);
    }

    public async Task<bool> UpdateCategoryAsync(string categoryId, string userId, string name)
    {
        var categoryDocument = await _categoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null)
        {
            return false;
        }

        categoryDocument.Name = name;
        await _categoriesRepository.UpdateCategoryAsync(categoryDocument);

        return true;
    }

    public async Task<CategoryDocument> GetCategoryAsync(string categoryId, string userId)
    {
        return await _categoriesRepository.GetCategoryAsync(categoryId, userId);
    }

    public async Task<List<CategoryDocument>> ListCategoriesAsync(string userId)
    {
        return await _categoriesRepository.ListCategoriesAsync(userId);
    }

    public async Task<bool> UpdateCategoryImageAsync(string categoryId, string userId)
    {
        var categoryDocument = await _categoriesRepository.GetCategoryAsync(categoryId, userId);
        if (categoryDocument == null)
        {
            return false;
        }

        var imageUrl = await _imageSearchService.FindImageUrlAsync(categoryDocument.Name);
        if (string.IsNullOrEmpty(imageUrl)) return false;

        // get the document again, to reduce the likelihood of concurrency races
        categoryDocument = await _categoriesRepository.GetCategoryAsync(categoryId, userId);

        categoryDocument.ImageUrl = imageUrl;
        await _categoriesRepository.UpdateCategoryAsync(categoryDocument);

        return true;
    }
}