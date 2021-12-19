using System.Threading.Tasks;
using FunctionAppExample.Models;
using FunctionAppExample.ResponseDtos;

namespace FunctionAppExample.Repositories;

public interface ICategoriesRepository
{
    Task<string> AddCategoryAsync(CategoryDocument categoryObject);
    Task<DeleteCategoryResult> DeleteCategoryAsync(string categoryId, string userId);
    Task UpdateCategoryAsync(CategoryDocument categoryDocument);
    Task<CategoryDocument> GetCategoryAsync(string categoryId, string userId);
    Task<CategorySummariesResponse> ListCategoriesAsync(string userId);
    Task<CategoryDocument> FindCategoryWithItemAsync(string itemId, ItemType itemType, string userId);
}