namespace PatHead.StorageService.Util.S3.Model
{
    public class AwsS3ConfigModel
    {
        public bool UseHttp { get; set; } = true;
        public bool ForcePathStyle { get; set; } = true;
        public string SignatureVersion { get; set; } = "2";
    }
}