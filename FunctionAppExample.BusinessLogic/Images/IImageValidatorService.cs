namespace FunctionAppExample.BusinessLogic.Images;

public interface IImageValidatorService
{
    (bool isValid, string mimeType) ValidateImage(Stream imageStream);
}