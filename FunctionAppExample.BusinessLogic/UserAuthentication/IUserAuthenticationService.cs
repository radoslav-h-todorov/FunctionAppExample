using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FunctionAppExample.BusinessLogic.UserAuthentication;

public interface IUserAuthenticationService
{
    Task<bool> GetUserIdAsync(HttpRequest req, out string userId, out IActionResult responseResult);
}