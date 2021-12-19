using SixLabors.ImageSharp;

namespace FunctionAppExample.BusinessLogic.Images;

public class ImageValidatorService : IImageValidatorService
{
    public (bool isValid, string mimeType) ValidateImage(Stream imageStream)
    {
        if (imageStream == null || imageStream.Length == 0)
        {
            return (false, null);
        }

        var imageFormat = Image.DetectFormat(imageStream);
        if (imageFormat == null)
        {
            return (false, null);
        }

        return (true, imageFormat.DefaultMimeType);
    }
}