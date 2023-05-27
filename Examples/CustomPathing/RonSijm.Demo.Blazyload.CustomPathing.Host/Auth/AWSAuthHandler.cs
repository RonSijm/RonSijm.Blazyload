using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using RegionEndpoint = Amazon.RegionEndpoint;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;

public class AWSAuthHandler
{
    private string _bucketUrl;
    private string _accessKey;
    private string _secretKey;

    public void SetCredentials(string bucketUrl, string accessKey, string secretKey)
    {
        _bucketUrl = bucketUrl;
        _accessKey = accessKey;
        _secretKey = secretKey;
    }

    public bool HandleAuth(string assembly, HttpRequestMessage httpMessage, BlazyAssemblyOptions assemblyOptions)
    {
        if (_accessKey == null || _secretKey == null)
        {
            return false;
        }

        if (httpMessage.RequestUri == null)
        {
            return false;
        }

        var bucketUri = new Uri(_bucketUrl);
        var hostSplit = bucketUri.Host.Split('.');
        var bucket = hostSplit[0];
        var regionName = hostSplit[2];

        var region = RegionEndpoint.GetBySystemName(regionName);

        var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
        var s3Client = new AmazonS3Client(credentials, region);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = assembly,
            Expires = DateTime.Now.AddMinutes(1)
        };

        // Generate the pre-signed URL
        var url = s3Client.GetPreSignedURL(request);

        httpMessage.RequestUri = new Uri(url);

        return true;
    }
}