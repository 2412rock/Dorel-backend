using DorelAppBackend.Services.Interface;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Net;

namespace DorelAppBackend.Services.Implementation
{
    public class BlobStorageService: IBlobStorageService
    {

        private IMinioClient minio;

        public BlobStorageService()
        {
            SetupMinio();
        }

        private string ResolveIp()
        {
            string hostIp;
            IPAddress[] addresses = Dns.GetHostAddresses("host.docker.internal");
            if (addresses.Length > 0)
            {
                // we are running in docker
                hostIp = addresses[0].ToString();
            }
            else
            {
                throw new Exception("No local ip found in docker dns");
            }

            return hostIp;
        }

        private void SetupMinio()
        {
            var endpoint = $"{ResolveIp()}:9000";
            var accessKey = "minioadmin";
            var secretKey = Environment.GetEnvironmentVariable("MINIO_PASS");
            try
            {
                this.minio = new MinioClient()
                                    .WithEndpoint(endpoint)
                                    .WithCredentials(accessKey, secretKey)
                                    .Build();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task UploadImage(string fileName, string fileExtension, string fileType, string fileContentBase64)
        {
            var bucketName = "servicii-and-pictures";
            //var location = "us-east-1";
            var objectName = fileName;
            
            var contentType = fileType;

            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket.
                string base64Data = fileContentBase64.Split(',')[1];

                byte[] base64Bytes = Convert.FromBase64String(base64Data);
                using (var stream = new MemoryStream(base64Bytes))
                {
                    var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);
                    await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                }

                /*var getObj = new GetObjectArgs().WithBucket(bucketName).WithObject(objectName).WithCallbackStream((stream) =>
                {
                    stream.CopyTo(Console.OpenStandardOutput());
                }); ;
                var download =  minio.GetObjectAsync(getObj);
                var x = 1;*/
                
                Console.WriteLine("Successfully uploaded " + objectName);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
                throw;
            }
        }
    }
}
