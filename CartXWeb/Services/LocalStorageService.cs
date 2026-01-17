
namespace CartXWeb.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LocalStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(string filename, Stream fileStream)
        {
            try
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string productPath = Path.Combine("images", "products", filename);
                string finalPath = Path.Combine(wwwRootPath, productPath);
                
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(finalPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save the file
                using (var fileStreamLocal = new FileStream(finalPath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(fileStreamLocal);
                }

                // Return the relative path for storage in database
                return @"\" + productPath.Replace(Path.DirectorySeparatorChar, '\\');
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file {filename} to local storage: {ex.Message}", ex);
            }
        }

        public async Task DeleteFileAsync(string filename)
        {
            try
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                // filename is in format like "\images\products\product-1\guid.ext"
                var filePath = Path.Combine(wwwRootPath, filename.TrimStart('\\'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting file {filename} from local storage: {ex.Message}", ex);
            }
        }

        public async Task<string> GetFileUrlAsync(string filename)
        {
            try
            {
                // filename is in format like "\images\products\product-1\guid.ext"
                // Return it as-is since it's already a relative path
                return await Task.FromResult(filename);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting URL for file {filename}: {ex.Message}", ex);
            }
        }
    }
}
