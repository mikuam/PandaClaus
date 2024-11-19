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

    public BlobClient(IConfiguration configuration)
    {
        var blobUrl = configuration["BlobUrl"];
        var blobSasToken = configuration["BlobSasToken"];

        _blobServiceClient = new BlobServiceClient(new Uri($"{blobUrl}?{blobSasToken}"));
    }

    public async Task<List<string>> UploadPhotos(List<IFormFile> files)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("photos2024");
        var imageIds = new List<string>();
        foreach (var file in files)
        {
            var imageId = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var thumbnailId = imageId + "_thumbnail" + ".jpg";  // Use .jpg extension for thumbnails

            await containerClient.UploadBlobAsync(imageId, file.OpenReadStream());

            var blobHttpHeader = new BlobHttpHeaders { ContentType = ImageHelper.GetContentType(file.FileName) };
            await containerClient.GetBlobClient(imageId).SetHttpHeadersAsync(blobHttpHeader);

            imageIds.Add(imageId);

            await GenerateAndUploadThumbnail(file, containerClient, thumbnailId);
        }

        return imageIds;
    }

    private static async Task GenerateAndUploadThumbnail(IFormFile file, BlobContainerClient containerClient,
        string thumbnailId)
    {
        using var image = await Image.LoadAsync(file.OpenReadStream());
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

    internal async Task UpdateContentType(string image, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("photos2024");
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
