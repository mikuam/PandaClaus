using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PandaClaus.Web.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PandaClaus.Web;

public class BlobClient
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _blobContainerName;
    private readonly ILogger<BlobClient> _logger;

    public BlobClient(IConfiguration configuration, ILogger<BlobClient> logger)
    {
        var blobUrl = configuration["BlobUrl"];
        var blobSasToken = configuration["BlobContainerSasToken"];
        _blobContainerName = configuration["BlobContainerName"] ?? "photos2024";
        _logger = logger;

        _blobServiceClient = new BlobServiceClient(new Uri($"{blobUrl}?{blobSasToken}"));
    }

    public async Task<List<string>> UploadPhotos(List<IFormFile> files)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        var imageIds = new List<string>();
        foreach (var file in files)
        {
            var imageId = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var thumbnailId = imageId + "_thumbnail" + ".jpg";  // Use .jpg extension for thumbnails

            _logger.LogInformation("Uploading photo: FileName={FileName}, ImageId={ImageId}, ContentType={ContentType}, Length={Length}", 
                file.FileName, imageId, file.ContentType, file.Length);

            await using (var cleanStream = await StripMetadata(file))
            {
                await containerClient.UploadBlobAsync(imageId, cleanStream);
            }

            var blobHttpHeader = new BlobHttpHeaders { ContentType = ImageHelper.GetContentType(file.FileName) };
            await containerClient.GetBlobClient(imageId).SetHttpHeadersAsync(blobHttpHeader);

            imageIds.Add(imageId);

            try
            {
                await GenerateAndUploadThumbnail(file, containerClient, thumbnailId, imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate thumbnail for image. FileName={FileName}, ImageId={ImageId}, ThumbnailId={ThumbnailId}, ContentType={ContentType}, Length={Length}", 
                    file.FileName, imageId, thumbnailId, file.ContentType, file.Length);
                throw;
            }
        }

        return imageIds;
    }

    private static void ClearMetadata(Image image)
    {
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
    }

    private static async Task<MemoryStream> StripMetadata(IFormFile file)
    {
        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input);
        var format = image.Metadata.DecodedImageFormat ?? throw new InvalidOperationException("Unknown image format.");

        image.Mutate(x => x.AutoOrient());
        ClearMetadata(image);

        var output = new MemoryStream();
        await image.SaveAsync(output, format);
        output.Position = 0;
        return output;
    }

    private async Task GenerateAndUploadThumbnail(IFormFile file, BlobContainerClient containerClient,
        string thumbnailId, string imageId)
    {
        try
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.AutoOrient());
            ClearMetadata(image);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(600, 600)
            }));

            using var memoryStream = new MemoryStream();
            await image.SaveAsync(memoryStream, new JpegEncoder());
            memoryStream.Position = 0;  // Reset the stream position for uploading

            // Upload the thumbnail with content type
            await containerClient.UploadBlobAsync(thumbnailId, memoryStream);

            var blobHttpHeader = new BlobHttpHeaders { ContentType = ImageHelper.GetContentType(thumbnailId) };
            await containerClient.GetBlobClient(thumbnailId).SetHttpHeadersAsync(blobHttpHeader);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load or process image for thumbnail generation. FileName={FileName}, ImageId={ImageId}, ThumbnailId={ThumbnailId}, ContentType={ContentType}, Length={Length}", 
                file.FileName, imageId, thumbnailId, file.ContentType, file.Length);
            throw;
        }
    }

    internal async Task UpdateContentType(string image, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);
        var blobClient = containerClient.GetBlobClient(image);

        var blobProperties = await blobClient.GetPropertiesAsync();

        if (blobProperties.Value.ContentType.StartsWith("image"))
        {
            return;
        }

        // Set the updated headers back to the blob
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType, ContentHash = blobProperties.Value.ContentHash });
    }
}
