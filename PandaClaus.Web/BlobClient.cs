using Azure.Storage.Blobs;

namespace PandaClaus.Web;

public class BlobClient
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobClient(IConfiguration configuration)
    {
        var blobUrl = configuration["BlobUrl"];
        var containerSasToken = configuration["ContainerSasToken"];

        _blobServiceClient = new BlobServiceClient(new Uri($"{blobUrl}?{containerSasToken}"));
    }

    public async Task<List<string>> UploadPhotos(List<IFormFile> files)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient("photos");
        var imageIds = new List<string>();
        foreach (var file in files)
        {
            var imageId = Guid.NewGuid() + file.FileName;
            await containerClient.UploadBlobAsync(imageId, file.OpenReadStream());
            imageIds.Add(imageId);
        }

        return imageIds;
    }
}
