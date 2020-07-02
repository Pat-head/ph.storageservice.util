using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PatHead.StorageService.Util.S3;
using PatHead.StorageService.Util.S3.Model;

namespace PatHead.StorageService.Util.Test
{
    public class Tests
    {
        private AuthorizationModel _model;

        private const string FileFullName =
            "JXU4RkQ5JXU5MUNDJXU2NjJGJXU4OTgxJXU1MkEwJXU1QkM2JXU3Njg0JXU1MTg1JXU1QkI5JXVGRjAx.png";

        [SetUp]
        public void Setup()
        {
            _model = new AuthorizationModel()
            {
                AccessKey = "415WK7QG60FJHUGCE3TO",
                SecretAccessKey = "904nqDB1kcmPrzLWTNkjA4n3t77AU2JRJyqAKkNo",
                ServiceURL = "http://172.16.129.169:39900"
            };
        }


        [Test]
        public async Task TestStaticPushStream()
        {
            var currentDirectory = Environment.CurrentDirectory;
            Stream memoryStream = new MemoryStream();
            using (var stream = new FileStream($"{currentDirectory}//{FileFullName}", FileMode.OpenOrCreate))
            {
                byte[] array = new byte[stream.Length];
                await stream.ReadAsync(array, 0, array.Length);
                await memoryStream.WriteAsync(array, 0, array.Length);
            }

            var pushStream = await AwsS3Service.PushStream(
                FileFullName, "",
                memoryStream, _model);

            Assert.True(pushStream);
        }


        [Test]
        public async Task TestStaticGetStream()
        {
            var stream = await AwsS3Service.GetStream(
                "JXU4RkQ5JXU5MUNDJXU2NjJGJXU4OTgxJXU1MkEwJXU1QkM2JXU3Njg0JXU1MTg1JXU1QkI5JXVGRjAx.png", "", _model);
            Assert.True(stream.stream != null);
        }
    }
}