using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FunctionAppExample.BusinessLogic.Images;

public class ImagePreviewService : IImagePreviewService
{
    protected const int PreviewSize = 150;

    public Stream CreatePreviewImage(Stream inputStream)
    {
        using (var image = Image.Load(inputStream))
        {
            image.Mutate(x => x.Resize(PreviewSize, PreviewSize));

            var previewStream = new MemoryStream();
            image.Save(previewStream, new JpegEncoder());
            previewStream.Position = 0;
            return previewStream;
        }
    }
}