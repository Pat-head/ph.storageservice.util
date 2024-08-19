using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PatHead.StorageService.Util.Model;

namespace PatHead.StorageService.Util.Services
{
    public class AmazonS3StorageService : IStorageService
    {
        private readonly AmazonS3Client _amazonS3Client;

        public AmazonS3StorageService(IOptions<StorageConfig> options)
        {
            if (options == null) return;

            var optionsValue = options.Value;

            if (!string.IsNullOrEmpty(optionsValue.Endpoint))
            {
                var urlPrefix = optionsValue.SSL ? "https://" : "http://";

                AmazonS3Config config = new AmazonS3Config
                {
                    ServiceURL = urlPrefix + optionsValue.Endpoint,
                    UseHttp = true,
                    ForcePathStyle = true,
                    SignatureVersion = "2",
                    HttpClientFactory = new CustomHttpClientFactory()
                };

                _amazonS3Client = new AmazonS3Client(optionsValue.AccessKey, optionsValue.SecretKey, config);
            }
            else
            {
                _amazonS3Client = new AmazonS3Client(RegionEndpoint.CNNorth1);
            }
        }

        public async Task<List<BucketDTO>> GetAllBucketListAsync()
        {
            var listBucketsAsync = await _amazonS3Client.ListBucketsAsync(new ListBucketsRequest());

            return listBucketsAsync.Buckets.Select(x => new BucketDTO()
            {
                Name = x.BucketName,
                CreatedTime = x.CreationDate
            }).ToList();
        }

        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            var buckets = await GetAllBucketListAsync();
            return buckets.Any(x => x.Name == bucketName);
        }

        public async Task CreateBucketAsync(string bucketName)
        {
            if (!await BucketExistsAsync(bucketName))
            {
                await _amazonS3Client.PutBucketAsync(bucketName);
            }
        }

        public async Task DeleteBucketAsync(string bucketName)
        {
            if (!await BucketExistsAsync(bucketName))
            {
                await _amazonS3Client.DeleteBucketAsync(bucketName);
            }
        }

        public async Task PutObjectAsync(string bucketName, string objectName, byte[] bytes)
        {
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = objectName,
                InputStream = new MemoryStream(bytes)
            };

            await _amazonS3Client.PutObjectAsync(putObjectRequest);
        }

        public async Task GetObjectAsync(string bucketName, string objectName, MemoryStream outMemoryStream)
        {
            var getObjectRequest = new GetObjectRequest()
            {
                BucketName = bucketName,
                Key = objectName
            };

            var obj = await _amazonS3Client.GetObjectAsync(getObjectRequest);
            await obj.ResponseStream.CopyToAsync(outMemoryStream);
            outMemoryStream.Position = 0;
        }

        public async Task DeleteObjectAsync(string bucketName, string objectName)
        {
            DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest()
            {
                BucketName = bucketName,
                Key = objectName
            };
            await _amazonS3Client.DeleteObjectAsync(deleteObjectRequest);
        }
    }


    public class CustomHttpClientFactory : HttpClientFactory
    {
        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
        {
            // 信任所有证书的验证委托
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var httpClient = new HttpClient(httpClientHandler);

            return httpClient;
        }
    }
}