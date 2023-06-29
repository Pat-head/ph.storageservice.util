using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Minio;
using PatHead.StorageService.Util.Model;

namespace PatHead.StorageService.Util.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly MinioClient _minioClient;

        public MinioStorageService(IOptions<StorageConfig> options)
        {
            if (options == null) return;

            var optionsValue = options.Value;

            _minioClient = new MinioClient(
                optionsValue.Endpoint,
                optionsValue.AccessKey,
                optionsValue.SecretKey);
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            return await _minioClient.BucketExistsAsync(bucketName).ConfigureAwait(false);
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            var found = await _minioClient.BucketExistsAsync(bucketName).ConfigureAwait(false);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(bucketName).ConfigureAwait(false);
            }
        }

        public async Task PutObjectAsync(string bucketName, string fileName, byte[] bytes)
        {
            using (Stream stream = new MemoryStream(bytes))
            {
                await _minioClient.PutObjectAsync(bucketName, fileName, stream, stream.Length).ConfigureAwait(false);
            }
        }

        public Task GetObjectAsync(string bucketName, string fileName, MemoryStream memoryStream)
        {
            return _minioClient.GetObjectAsync(bucketName, fileName, stream =>
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
            });
        }

        public Task DeleteObjectAsync(string bucketName, string objectName)
        {
            return _minioClient.RemoveIncompleteUploadAsync(bucketName, objectName);
        }
    }
}