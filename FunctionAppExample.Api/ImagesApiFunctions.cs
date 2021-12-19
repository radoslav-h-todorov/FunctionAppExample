using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using FunctionAppExample.Api.RequestDtos;
using FunctionAppExample.BusinessLogic.Images;
using FunctionAppExample.BusinessLogic.UserAuthentication;
using FunctionAppExample.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppExample.Api;

public class ImagesApiFunctions
{
    private readonly ImagesService _imagesService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly ILogger<ImagesApiFunctions> _logger;

    public ImagesApiFunctions(ILogger<ImagesApiFunctions> logger)
    {
        _imagesService = new ImagesService(new BlobHelper(), new ImageValidatorService(), new ImagePreviewService());
        _userAuthenticationService = new QueryStringUserAuthenticationService();
        _logger = logger;
    }

    [FunctionName("BeginCreateImage")]
    public async Task<IActionResult> BeginCreateImage
        ([HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")] HttpRequest req)
    {
        // get the user ID
        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var (id, url) = _imagesService.BeginAddImageNote(userId);

            return new OkObjectResult(new
            {
                id = id,
                url = url
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }

    [FunctionName("CompleteCreateImage")]
    public async Task<IActionResult> CompleteCreateImage
        ([HttpTrigger(AuthorizationLevel.Function, "post", Route = "images/{id}")] HttpRequest req, string id)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        CompleteCreateImageRequest data;
        try
        {
            data = JsonConvert.DeserializeObject<CompleteCreateImageRequest>(requestBody);
        }
        catch (JsonReaderException)
        {
            return new BadRequestObjectResult(new { error = "Body should be provided in JSON format." });
        }

        if (data == null || string.IsNullOrEmpty(data.CategoryId))
        {
            return new BadRequestObjectResult(new { error = "Missing required property 'categoryId'." });
        }

        if (!await _userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
        {
            return responseResult;
        }

        try
        {
            var (result, previewUrl) = await _imagesService.CompleteAddImageNoteAsync(id, userId, data.CategoryId);

            if (result != null)
            {
                return new BadRequestObjectResult(new { error = result });
            }

            return new OkObjectResult(new
            {
                previewUrl = previewUrl
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception: {ex.Message}");
            return new ExceptionResult(ex, false);
        }
    }
}