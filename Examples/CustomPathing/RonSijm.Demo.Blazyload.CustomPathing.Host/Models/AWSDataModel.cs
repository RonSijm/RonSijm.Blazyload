using System.ComponentModel.DataAnnotations;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Models;

public class AWSDataModel
{
    [Required(ErrorMessage = "BucketUrl is required.")]
    public string BucketUrl { get; set; } = "https://blazyload.s3.eu-central-1.amazonaws.com/";

    [Required(ErrorMessage = "Access Key is required.")]
    public string AccessKey { get; set; }

    [Required(ErrorMessage = "Secret Key is required.")]
    public string SecretKey { get; set; }
}