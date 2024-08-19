namespace PatHead.StorageService.Util.Model
{
    public class StorageConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool SSL { get; set; } = false;
        public bool CertificateVerification { get; set; } = false;
    }
}