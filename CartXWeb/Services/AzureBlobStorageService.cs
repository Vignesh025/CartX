
using Azure.Storage.Blobs;

namespace CartXWeb.Services
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;

        public AzureBlobStorageService(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public async Task<string> UploadFileAsync(string filename, Stream fileStream)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(filename);
                await blobClient.UploadAsync(fileStream, overwrite: true);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file {filename} to blob storage: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string filename)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(filename);
                await blobClient.DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file {filename} from blob storage: {ex.Message}", ex);
            }
        }

        public async Task<string> GetFileUrlAsync(string filename)
        {
            try
            {
                BlobClient blobClient = _blobContainerClient.GetBlobClient(filename);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting URL for file {filename}: {ex.Message}", ex);
            }
        }
    }
}
