
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Movieadmin.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString =
                configuration["AzureStorage:ConnectionString"]!;

            string containerName =
                configuration["AzureStorage:ContainerName"]!;

            BlobServiceClient blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(containerName);

            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(
            Stream imageStream,
            string fileName,
            string contentType)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(imageStream, overwrite: true);

            await blobClient.SetHttpHeadersAsync(
                new BlobHttpHeaders
                {
                    ContentType = contentType
                });

            return blobClient.Uri.ToString();
        }

        public async Task<BlobDownloadResult> DownloadImageAsync(
            string imageUrl)
        {
            Uri uri = new Uri(imageUrl);
            string fileName = Uri.UnescapeDataString(uri.Segments[^1]);

            BlobClient blobClient =
                _containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadContentAsync();

            return response.Value;
        }
    }
}
