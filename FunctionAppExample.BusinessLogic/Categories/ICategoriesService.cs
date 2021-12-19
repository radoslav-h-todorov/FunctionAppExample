using FunctionAppExample.DataAccess.Models;

namespace FunctionAppExample.BusinessLogic.Categories;

public interface ICategoriesService
{
    Task<string> AddCategoryAsync(string name, string userId);
    Task<bool> DeleteCategoryAsync(string categoryId, string userId);
    Task<bool> UpdateCategoryAsync(string categoryId, string userId, string name);
    Task<CategoryDocument> GetCategoryAsync(string categoryId, string userId);
    Task<List<CategoryDocument>> ListCategoriesAsync(string userId);
    Task<bool> UpdateCategoryImageAsync(string categoryId, string userId);
}