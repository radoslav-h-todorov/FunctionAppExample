namespace FunctionAppExample.BusinessLogic.Images;

public interface IImagesService
{
    (string id, string url) BeginAddImageNote(string userId);

    Task<(string? result, string? previewUri)> CompleteAddImageNoteAsync(string imageId, string userId, string categoryId);
}