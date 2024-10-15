using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace PandaClaus.Web;

public class BlobClient
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public BlobClient(IConfiguration configuration)
    {
        var blobUrl = configuration["BlobUrl"];
        var containerSasToken = configuration["ContainerSasToken"];
        var storageAccountKey = configuration["StorageAccountKey"];

        StorageSharedKeyCredential storageSharedKeyCredential = new("pandaclaus", storageAccountKey);

        _blobServiceClient = new BlobServiceClient(new Uri($"{blobUrl}?{containerSasToken}"), storageSharedKeyCredential);
        _containerClient = _blobServiceClient.GetBlobContainerClient("photos");

    }

    public async Task<List<string>> UploadPhotos(List<IFormFile> files)
    {
        var imageIds = new List<string>();
        foreach (var file in files)
        {
            var imageId = Guid.NewGuid() + file.FileName;
            await _containerClient.UploadBlobAsync(imageId, file.OpenReadStream());
            imageIds.Add(imageId);
        }

        return imageIds;
    }

    public string GetBlobUriWithSasToken(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var expirationTime = DateTimeOffset.UtcNow.AddHours(1);

        var sasBuilder = new BlobSasBuilder
        {
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = expirationTime,
            Resource = "b", // 'b' for blob, 'c' for container
            BlobContainerName = "photos",
            BlobName = blobName,
            Protocol = SasProtocol.HttpsAndHttp
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task UpdateBlobContentType(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var properties = await blobClient.GetPropertiesAsync();

        var acceptedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = GetFileExtension(blobName).TrimStart('.').ToLower();
        if (acceptedExtensions.Contains(extension))
        {
            blobClient.SetHttpHeaders(new BlobHttpHeaders
            {
                ContentType = $"image/{extension}"
            });
        }
    }
    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName);
    }
}
