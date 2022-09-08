using System.IO;
using System.Threading.Tasks;

namespace PatHead.StorageService.Util.Services
{
    public interface IStorageService
    {
        Task<bool> BucketExistsAsync(string bucketName);

        Task CreateBucketAsync(string bucketName);

        Task PutObjectAsync(string bucketName, string objectName, byte[] bytes);

        Task GetObjectAsync(string bucketName, string objectName, MemoryStream outputMemoryStream);
    }
}