namespace BcatBotFramework.Core.Config
{
    public class S3Config : ISubConfiguration
    {
        public string ServiceUrl
        {
            get;
            set;
        }

        public string AccessKey
        {
            get;
            set;
        }

        public string AccessKeySecret
        {
            get;
            set;
        }

        public string BucketName
        {
            get;
            set;
        }

        public void SetDefaults()
        {
            ServiceUrl = "https://s3.example.com";
            BucketName = "bucket";
            AccessKey = "cafebabe";
            AccessKeySecret = "deadbeef";
        }
        
    }
}