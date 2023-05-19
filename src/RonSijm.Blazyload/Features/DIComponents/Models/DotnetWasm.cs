using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Features.DIComponents.Models;

public class DotnetWasm
{
    [JsonPropertyName("behavior")]
    public string Behavior { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}