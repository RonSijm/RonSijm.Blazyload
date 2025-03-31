// ReSharper disable global UnusedMember.Global

using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Loading;

public class RuntimeAssets
{
    [JsonPropertyName("dotnet.wasm")]
    public DotnetWasm DotnetWasm { get; set; }
}