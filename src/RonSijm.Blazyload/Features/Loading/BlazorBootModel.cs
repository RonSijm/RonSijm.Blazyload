// ReSharper disable global UnusedMember.Global

using System.Text.Json.Serialization;

namespace RonSijm.Blazyload.Loading;

public class BlazorBootModel
{
    [JsonPropertyName("cacheBootResources")]
    public bool CacheBootResources { get; set; }

    [JsonPropertyName("config")]
    public List<object> Config { get; set; }

    [JsonPropertyName("debugBuild")]
    public bool DebugBuild { get; set; }

    [JsonPropertyName("entryAssembly")]
    public string EntryAssembly { get; set; }

    [JsonPropertyName("icuDataMode")]
    public int IcuDataMode { get; set; }

    [JsonPropertyName("linkerEnabled")]
    public bool LinkerEnabled { get; set; }

    [JsonPropertyName("resources")]
    public Resources Resources { get; set; }
}