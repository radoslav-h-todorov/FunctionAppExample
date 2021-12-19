using System.Threading.Tasks;

namespace FunctionAppExample.Services;

public interface IImageSearchService
{
    Task<string> FindImageUrlAsync(string searchTerm);
}