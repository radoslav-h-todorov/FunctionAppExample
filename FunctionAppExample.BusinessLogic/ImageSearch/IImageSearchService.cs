namespace FunctionAppExample.BusinessLogic.ImageSearch;

public interface IImageSearchService
{
    Task<string> FindImageUrlAsync(string searchTerm);
}