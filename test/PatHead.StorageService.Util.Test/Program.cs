using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PatHead.StorageService.Util.DependencyInjection;
using PatHead.StorageService.Util.Services;

namespace PatHead.StorageService.Util.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();

            ServiceCollection collection = new ServiceCollection();
            collection.AddMinioStorageService(options =>
            {
                options.Endpoint = configuration["Minio:URL"];
                options.AccessKey = configuration["Minio:KEY"];
                options.SecretKey = configuration["Minio:SECRET"];
                options.SSL = configuration.GetValue<bool>("Minio:SSL");
            });
            collection.AddTransient<TestService>();
            var buildServiceProvider = collection.BuildServiceProvider();

            var testService = buildServiceProvider.GetService<TestService>();
            testService.DoIt().GetAwaiter().GetResult();
        }
    }

    public class TestService
    {
        private readonly IServiceProvider _serviceProvider;

        public TestService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DoIt()
        {
            var storageFactoryService = _serviceProvider.GetService<StorageFactoryService>();
            using var stmMemory = new MemoryStream();

            var minioStorageService = storageFactoryService.GetStorageService<MinioStorageService>();

            var allBucketListAsync = await minioStorageService.GetAllBucketListAsync();

            const string bucketName = "demo-test";

            var bucketExistsAsync = await minioStorageService.BucketExistsAsync(bucketName);

            if (!bucketExistsAsync) Console.WriteLine("bucket not exist");

            await minioStorageService.CreateBucketAsync(bucketName);

            var bucketExists2Async = await minioStorageService.BucketExistsAsync(bucketName);

            if (bucketExists2Async) Console.WriteLine("bucket create");

            const string fileName = "test.txt";

            {
                using MemoryStream memoryStream = new MemoryStream();
                await using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    await streamWriter.WriteAsync("hello minio");
                    await streamWriter.FlushAsync();
                }

                await minioStorageService.PutObjectAsync(bucketName, fileName, memoryStream.ToArray());
                Console.WriteLine("pull test.txt to bucket");
            }

            {
                using MemoryStream memoryStream = new MemoryStream();
                await minioStorageService.GetObjectAsync(bucketName, fileName, memoryStream);
                Console.WriteLine("get test.txt from bucket:");
                await memoryStream.CopyToAsync(Console.OpenStandardOutput());
            }

            await minioStorageService.DeleteObjectAsync(bucketName, fileName);
            Console.WriteLine("delete test.txt");

            await minioStorageService.DeleteBucketAsync(bucketName);
            Console.WriteLine("delete bucket");
        }
    }
}