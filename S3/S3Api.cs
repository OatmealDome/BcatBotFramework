using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BcatBotFramework.Core.Config;

namespace S3
{
    public class S3Api
    {
        // Instances
        private static AmazonS3Client s3Client;
        private static TransferUtility transferUtility;
        private static bool initialized = false;

        public static void Initialize()
        {
            // Create a new AmazonS3Client
            s3Client = new AmazonS3Client(Configuration.LoadedConfiguration.S3Config.AccessKey, Configuration.LoadedConfiguration.S3Config.AccessKeySecret, new AmazonS3Config
            {
                ServiceURL = Configuration.LoadedConfiguration.S3Config.ServiceUrl
            });

            // Create a TransferUtility instance
            transferUtility = new TransferUtility(s3Client);

            // Set initialized flag
            initialized = true;
        }

        public static void Dispose()
        {
            transferUtility.Dispose();
            s3Client.Dispose();
            initialized = false;
        }

        public static void TransferFile(byte[] data, string remoteDirectory, string remoteFile)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                TransferFile(memoryStream, remoteDirectory, remoteFile);
            }
        }

        public static void TransferFile(Stream inputStream, string remoteDirectory, string remoteFile)
        {
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest()
            {
                BucketName = Configuration.LoadedConfiguration.S3Config.BucketName + remoteDirectory,
                Key = remoteFile,
                InputStream = inputStream,
                CannedACL = S3CannedACL.PublicRead,
                AutoResetStreamPosition = true,
                AutoCloseStream = false
            };

            TransferFile(request);
        }

        public static void TransferFile(string localPath, string remoteDirectory)
        {
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest()
            {
                BucketName = Configuration.LoadedConfiguration.S3Config.BucketName + remoteDirectory,
                Key = Path.GetFileName(localPath),
                FilePath = localPath,
                CannedACL = S3CannedACL.PublicRead
            };

            TransferFile(request);
        }

        public static void TransferFile(TransferUtilityUploadRequest request)
        {
            // Check if initialized
            if (!initialized)
            {
                throw new Exception("Cannot access S3 instance while uninitialized");
            } 
            
            // Declare a retry counter
            int retries = 10;

            // Declare the timeout value
            int timeout = 5;

            while (true)
            {
                // Check the number of retries
                if (retries < 0)
                {
                    throw new Exception("Upload failure");
                }
                
                try 
                {
                    // Attempt the upload
                    transferUtility.Upload(request);

                    // Exit the loop
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"WARNING: Upload failed, waiting {timeout} seconds (retries remaining = {retries})");
                    Console.WriteLine(e.ToString());

                    // Wait for the timeout
                    Thread.Sleep(timeout * 1000);

                    // Decrement the retries
                    retries--;

                    // Increment the timeout
                    timeout += 5;
                }
                
            }
        }

    }
}