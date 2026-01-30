using AwesomeAssertions;
using RonSijm.Blazyload.Loading;
using System.Text.Json;

namespace RonSijm.Blazyload.Tests;

public class LoadingModelsTests
{
    [Fact]
    public void BlazorBootModel_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""cacheBootResources"": true,
            ""config"": [],
            ""debugBuild"": false,
            ""entryAssembly"": ""MyApp"",
            ""icuDataMode"": 0,
            ""linkerEnabled"": true,
            ""resources"": {
                ""assembly"": {},
                ""lazyAssembly"": {},
                ""pdb"": {},
                ""runtime"": {},
                ""runtimeAssets"": {
                    ""dotnet.wasm"": {
                        ""behavior"": ""dotnetwasm"",
                        ""hash"": ""sha256-abc123""
                    }
                }
            }
        }";

        // Act
        var model = JsonSerializer.Deserialize<BlazorBootModel>(json);

        // Assert
        model.Should().NotBeNull();
        model.CacheBootResources.Should().BeTrue();
        model.DebugBuild.Should().BeFalse();
        model.EntryAssembly.Should().Be("MyApp");
        model.IcuDataMode.Should().Be(0);
        model.LinkerEnabled.Should().BeTrue();
        model.Resources.Should().NotBeNull();
    }

    [Fact]
    public void Resources_ShouldDeserializeAssemblyDictionary()
    {
        // Arrange
        var json = @"{
            ""assembly"": {
                ""System.Runtime.wasm"": ""sha256-hash1"",
                ""MyApp.wasm"": ""sha256-hash2""
            },
            ""lazyAssembly"": {},
            ""pdb"": {},
            ""runtime"": {}
        }";

        // Act
        var resources = JsonSerializer.Deserialize<Resources>(json);

        // Assert
        resources.Should().NotBeNull();
        resources.Assembly.Should().NotBeNull();
        resources.Assembly.Should().HaveCount(2);
        resources.Assembly["System.Runtime.wasm"].Should().Be("sha256-hash1");
        resources.Assembly["MyApp.wasm"].Should().Be("sha256-hash2");
    }

    [Fact]
    public void Resources_ShouldDeserializeLazyAssemblyDictionary()
    {
        // Arrange
        var json = @"{
            ""assembly"": {},
            ""lazyAssembly"": {
                ""LazyLib1.wasm"": ""sha256-lazy1"",
                ""LazyLib2.wasm"": ""sha256-lazy2""
            },
            ""pdb"": {},
            ""runtime"": {}
        }";

        // Act
        var resources = JsonSerializer.Deserialize<Resources>(json);

        // Assert
        resources.Should().NotBeNull();
        resources.LazyAssembly.Should().NotBeNull();
        resources.LazyAssembly.Should().HaveCount(2);
        resources.LazyAssembly["LazyLib1.wasm"].Should().Be("sha256-lazy1");
        resources.LazyAssembly["LazyLib2.wasm"].Should().Be("sha256-lazy2");
    }

    [Fact]
    public void Resources_ShouldDeserializePdbDictionary()
    {
        // Arrange
        var json = @"{
            ""assembly"": {},
            ""lazyAssembly"": {},
            ""pdb"": {
                ""MyApp.pdb"": ""sha256-pdb1""
            },
            ""runtime"": {}
        }";

        // Act
        var resources = JsonSerializer.Deserialize<Resources>(json);

        // Assert
        resources.Should().NotBeNull();
        resources.Pdb.Should().NotBeNull();
        resources.Pdb.Should().HaveCount(1);
        resources.Pdb["MyApp.pdb"].Should().Be("sha256-pdb1");
    }

    [Fact]
    public void Resources_ShouldDeserializeRuntimeDictionary()
    {
        // Arrange
        var json = @"{
            ""assembly"": {},
            ""lazyAssembly"": {},
            ""pdb"": {},
            ""runtime"": {
                ""dotnet.wasm"": ""sha256-runtime1""
            }
        }";

        // Act
        var resources = JsonSerializer.Deserialize<Resources>(json);

        // Assert
        resources.Should().NotBeNull();
        resources.Runtime.Should().NotBeNull();
        resources.Runtime.Should().HaveCount(1);
        resources.Runtime["dotnet.wasm"].Should().Be("sha256-runtime1");
    }

    [Fact]
    public void RuntimeAssets_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""dotnet.wasm"": {
                ""behavior"": ""dotnetwasm"",
                ""hash"": ""sha256-abc123""
            }
        }";

        // Act
        var runtimeAssets = JsonSerializer.Deserialize<RuntimeAssets>(json);

        // Assert
        runtimeAssets.Should().NotBeNull();
        runtimeAssets.DotnetWasm.Should().NotBeNull();
    }

    [Fact]
    public void DotnetWasm_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""behavior"": ""dotnetwasm"",
            ""hash"": ""sha256-abc123""
        }";

        // Act
        var dotnetWasm = JsonSerializer.Deserialize<DotnetWasm>(json);

        // Assert
        dotnetWasm.Should().NotBeNull();
        dotnetWasm.Behavior.Should().Be("dotnetwasm");
        dotnetWasm.Hash.Should().Be("sha256-abc123");
    }
}

