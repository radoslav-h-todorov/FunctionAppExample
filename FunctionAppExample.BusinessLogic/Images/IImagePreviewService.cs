namespace FunctionAppExample.BusinessLogic.Images;

public interface IImagePreviewService
{
    Stream CreatePreviewImage(Stream inputStream);
}