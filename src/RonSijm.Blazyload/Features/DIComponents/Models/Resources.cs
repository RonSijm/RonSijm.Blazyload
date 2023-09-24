// ReSharper disable global UnusedMember.Global

using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Features.DIComponents.Models;

public class Resources
{
    [JsonPropertyName("assembly")]
    public Dictionary<string, string> Assembly { get; set; }

    [JsonPropertyName("extensions")]
    public object Extensions { get; set; }

    [JsonPropertyName("lazyAssembly")]
    public Dictionary<string, string> LazyAssembly { get; set; }

    [JsonPropertyName("libraryInitializers")]
    public object LibraryInitializers { get; set; }

    [JsonPropertyName("pdb")]
    public Dictionary<string, string> Pdb { get; set; }

    [JsonPropertyName("runtime")]
    public Dictionary<string, string> Runtime { get; set; }

    [JsonPropertyName("runtimeAssets")]
    public RuntimeAssets RuntimeAssets { get; set; }

    [JsonPropertyName("satelliteResources")]
    public object SatelliteResources { get; set; }
}