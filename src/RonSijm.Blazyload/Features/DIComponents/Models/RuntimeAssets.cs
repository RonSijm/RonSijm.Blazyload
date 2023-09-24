// ReSharper disable global UnusedMember.Global

using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Features.DIComponents.Models;

public class RuntimeAssets
{
    [JsonPropertyName("dotnet.wasm")]
    public DotnetWasm DotnetWasm { get; set; }
}