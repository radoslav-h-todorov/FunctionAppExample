using FunctionAppExample.DataAccess.Models;

namespace FunctionAppExample.DataAccess.Repositories;

public interface ICategoriesRepository
{
    Task<string> AddCategoryAsync(CategoryDocument categoryObject);
    Task<bool> DeleteCategoryAsync(string categoryId, string userId);
    Task UpdateCategoryAsync(CategoryDocument categoryDocument);
    Task<CategoryDocument> GetCategoryAsync(string categoryId, string userId);
    Task<List<CategoryDocument>> ListCategoriesAsync(string userId);
}