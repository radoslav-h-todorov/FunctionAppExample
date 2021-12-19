using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using FunctionAppExample.DataAccess;

namespace FunctionAppExample.BusinessLogic.Images;

public class ImagesService : IImagesService
{
    private readonly BlobHelper _blobHelper;
    private readonly IImageValidatorService _imageValidatorService;
    private readonly IImagePreviewService _imagePreviewService;

    private const string FullImagesBlobContainerName = "fullimages";
    private const string PreviewImagesBlobContainerName = "previewimages";
    private const string CategoryIdMetadataName = "categoryId";
    private const string UserIdMetadataName = "userId";
    private const long MaximumImageSize = 4L * 1024L * 1024L;

    public ImagesService(BlobHelper blobHelper, IImageValidatorService imageValidatorService, IImagePreviewService imagePreviewService)
    {
        _blobHelper = blobHelper;
        _imageValidatorService = imageValidatorService;
        _imagePreviewService = imagePreviewService;
    }

    public (string id, string url) BeginAddImageNote(string userId)
    {
        // generate an ID for this image note
        string imageId = Guid.NewGuid().ToString();

        // create a blob placeholder (which will not have any contents yet)
        BlockBlobClient blob = _blobHelper.GetBlobClient(FullImagesBlobContainerName, imageId);

        string urlAndSas = _blobHelper.GetSasUriForBlob(blob, BlobSasPermissions.Create | BlobSasPermissions.Write);

        return (imageId, urlAndSas);
    }

    public async Task<(string? result, string? previewUri)> CompleteAddImageNoteAsync(string imageId, string userId,
        string categoryId)
    {
        BlockBlobClient imageBlob = _blobHelper.GetBlobClient(FullImagesBlobContainerName, imageId);
        if (imageBlob == null || !await imageBlob.ExistsAsync())
        {
            // the blob hasn't actually been uploaded yet, so we can't process it
            return ("Image not uploaded", null);
        }

        using (var rawImage = new MemoryStream())
        {
            // get the image that was uploaded by the client
            await imageBlob.DownloadToAsync(rawImage);
            if (rawImage.CanSeek)
            {
                rawImage.Position = 0;
            }

            // if the blob already contains metadata then that means it has already been added
            Response<BlobProperties> response = await imageBlob.GetPropertiesAsync();
            if (response.Value.Metadata.ContainsKey(CategoryIdMetadataName))
            {
                return ("Image already created", null);
            }

            // validate the size of the image
            if (rawImage.Length > MaximumImageSize) // TODO confirm this works
            {
                return ("Image too large", null);
            }

            // validate the image is in an acceptable format
            var validationResult = _imageValidatorService.ValidateImage(rawImage);
            if (!validationResult.isValid)
            {
                return ("Invalid image", null);
            }

            if (rawImage.CanSeek)
            {
                rawImage.Position = 0;
            }

            // set the blob metadata
            var metadata = new Dictionary<string, string>
            {
                {CategoryIdMetadataName, categoryId},
                {UserIdMetadataName, userId},
            };

            await imageBlob.SetMetadataAsync(metadata);

            // create and upload a preview image for this blob
            BlockBlobClient previewImageBlob;
            using (var previewImageStream = _imagePreviewService.CreatePreviewImage(rawImage))
            {
                previewImageBlob = _blobHelper.GetBlobClient(PreviewImagesBlobContainerName, imageId);
                await previewImageBlob.UploadAsync(previewImageStream);
            }

            string previewUrlAndSas = _blobHelper.GetSasUriForBlob(previewImageBlob, BlobSasPermissions.Read);

            return (null, previewUrlAndSas);
        }
    }
}