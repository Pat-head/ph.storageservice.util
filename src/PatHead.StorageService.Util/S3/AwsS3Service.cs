using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using PatHead.StorageService.Util.S3.Model;

namespace PatHead.StorageService.Util.S3
{
    public interface IAwsS3Service
    {
    }


    public class AwsS3Service : IAwsS3Service
    {
        public static async Task<(string fileName, string contentType, Stream stream)> GetStream(
            string fileFullName, string bucketName,
            AuthorizationModel model, AwsS3ConfigModel configModel = null)
        {
            if (model == null)
            {
                throw new Exception("model is null");
            }

            if (configModel == null)
            {
                configModel = new AwsS3ConfigModel();
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                bucketName = "storage";
            }

            AmazonS3Config config = new AmazonS3Config
            {
                ServiceURL = model.ServiceURL,
                UseHttp = configModel.UseHttp,
                ForcePathStyle = configModel.ForcePathStyle,
                SignatureVersion = configModel.SignatureVersion
            };

            using (var s3Client = new AmazonS3Client(model.AccessKey, model.SecretAccessKey, config))
            {
                var request = new GetObjectRequest {BucketName = bucketName, Key = fileFullName};
                var response = await s3Client.GetObjectAsync(request);
                return (fileFullName, MimeMapping.MimeUtility.GetMimeMapping(fileFullName), response.ResponseStream);
            }
        }

        public static async Task<bool> PushStream(string fileFullName, string bucketName,
            Stream stream,
            AuthorizationModel model, AwsS3ConfigModel configModel = null)
        {
            if (model == null)
            {
                throw new Exception("model is null");
            }

            if (configModel == null)
            {
                configModel = new AwsS3ConfigModel();
            }

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                bucketName = "storage";
            }

            AmazonS3Config config = new AmazonS3Config
            {
                ServiceURL = model.ServiceURL,
                UseHttp = configModel.UseHttp,
                ForcePathStyle = configModel.ForcePathStyle,
                SignatureVersion = configModel.SignatureVersion
            };

            try
            {
                using (var s3Client = new AmazonS3Client(model.AccessKey, model.SecretAccessKey, config))
                {
                    var request = new PutObjectRequest
                        {BucketName = bucketName, Key = fileFullName, InputStream = stream};
                    await s3Client.PutObjectAsync(request);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.Write(e);
                return false;
            }
        }
    }
}