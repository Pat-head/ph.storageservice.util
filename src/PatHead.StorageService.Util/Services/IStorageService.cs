using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PatHead.StorageService.Util.Model;

namespace PatHead.StorageService.Util.Services
{
    public interface IStorageService
    {
        Task<List<BucketDTO>> GetAllBucketListAsync();

        Task<bool> BucketExistsAsync(string bucketName);

        Task CreateBucketAsync(string bucketName);

        Task DeleteBucketAsync(string bucketName);

        Task PutObjectAsync(string bucketName, string objectName, byte[] bytes);

        Task GetObjectAsync(string bucketName, string objectName, MemoryStream outMemoryStream);

        Task DeleteObjectAsync(string bucketName, string objectName);
    }
}