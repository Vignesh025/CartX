using System.IO;
using System.Threading.Tasks;

namespace CartXWeb.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(string filename, Stream fileStream);
        Task DeleteFileAsync(string filename);
        Task<string> GetFileUrlAsync(string filename);
    }
}
