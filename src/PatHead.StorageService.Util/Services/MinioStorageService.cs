using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            _minioClient = new MinioClient()
                .WithEndpoint(optionsValue.Endpoint)
                .WithCredentials(optionsValue.AccessKey, optionsValue.SecretKey);

            if (optionsValue.SSL)
            {
                _minioClient.WithSSL();

                if (!optionsValue.CertificateVerification)
                {
                    _minioClient.WithHttpClient(new HttpClient(new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    }));
                }
            }
            _minioClient = _minioClient.Build();
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            return await _minioClient.BucketExistsAsync(bucketExistsArgs).ConfigureAwait(false);
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            var found = await BucketExistsAsync(bucketName).ConfigureAwait(false);
            if (!found)
            {
                MakeBucketArgs args = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(args).ConfigureAwait(false);
            }
        }

        public Task DeleteBucketAsync(string bucketName)
        {
            return _minioClient.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucketName));
        }

        public async Task PutObjectAsync(string bucketName, string fileName, byte[] bytes)
        {
            using (Stream stream = new MemoryStream(bytes))
            {
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithObject(fileName);

                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            }
        }

        public async Task GetObjectAsync(string bucketName, string fileName, MemoryStream outMemoryStream)
        {
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(outMemoryStream);
                    outMemoryStream.Position = 0;
                });

            await _minioClient.GetObjectAsync(getObjectArgs);
        }

        public Task DeleteObjectAsync(string bucketName, string objectName)
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            return _minioClient.RemoveObjectAsync(removeObjectArgs);
        }

        public async Task<List<BucketDTO>> GetAllBucketListAsync()
        {
            var listBucketsAsync = await _minioClient.ListBucketsAsync();

            var bucket = listBucketsAsync.Buckets.Select(x => new BucketDTO()
            {
                Name = x.Name,
                CreatedTime = x.CreationDateDateTime
            }).ToList();

            return bucket;
        }
    }
}