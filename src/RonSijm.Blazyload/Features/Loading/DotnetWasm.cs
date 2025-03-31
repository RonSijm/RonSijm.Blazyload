// ReSharper disable global UnusedMember.Global

using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Loading;

public class DotnetWasm
{
    [JsonPropertyName("behavior")]
    public string Behavior { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}