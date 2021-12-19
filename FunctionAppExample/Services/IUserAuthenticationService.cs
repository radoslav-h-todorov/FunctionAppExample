using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FunctionAppExample.Services;

public interface IUserAuthenticationService
{
    Task<bool> GetUserIdAsync(HttpRequest req, out string userId, out IActionResult responseResult);
}