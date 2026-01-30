using AwesomeAssertions;

namespace RonSijm.Blazyload.Tests;

public class BlazyloadProviderOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeAssemblyLoaderOptions()
    {
        // Act
        var options = new BlazyloadProviderOptions();

        // Assert
        options.AssemblyLoaderOptions.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeAssemblyLoadConfiguration()
    {
        // Act
        var options = new BlazyloadProviderOptions();

        // Assert - Using reflection to access internal property
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options);
        config.Should().NotBeNull();
    }

    [Fact]
    public void LoadOnNavigation_ShouldAddPathAndAssemblyToConfiguration()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var path = "fetchdata1";
        var assembly = "MyAssembly.wasm";

        // Act
        var result = options.LoadOnNavigation(path, assembly);

        // Assert
        result.Should().Be(options); // Should return itself for fluent API
        
        // Verify it was added to configuration
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options) as AssemblyLoadConfiguration;
        config?.GetAssembly(path).Should().Be(assembly);
    }

    [Fact]
    public void LoadOnNavigation_ShouldSupportFluentChaining()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();

        // Act
        var result = options
            .LoadOnNavigation("path1", "Assembly1.wasm")
            .LoadOnNavigation("path2", "Assembly2.wasm")
            .LoadOnNavigation("path3", "Assembly3.wasm");

        // Assert
        result.Should().Be(options);
        
        // Verify all were added
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options) as AssemblyLoadConfiguration;
        config?.GetAssembly("path1").Should().Be("Assembly1.wasm");
        config?.GetAssembly("path2").Should().Be("Assembly2.wasm");
        config?.GetAssembly("path3").Should().Be("Assembly3.wasm");
    }

    [Fact]
    public void LoadOnNavigation_ShouldHandleMultipleCalls()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();

        // Act
        for (int i = 0; i < 10; i++)
        {
            options.LoadOnNavigation($"path{i}", $"Assembly{i}.wasm");
        }

        // Assert
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options) as AssemblyLoadConfiguration;
        
        for (int i = 0; i < 10; i++)
        {
            config?.GetAssembly($"path{i}").Should().Be($"Assembly{i}.wasm");
        }
    }

    [Fact]
    public void AssemblyLoaderOptions_ShouldBeAccessible()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();

        // Act
        options.AssemblyLoaderOptions.DisableCascadeLoading = true;
        options.AssemblyLoaderOptions.EnableLogging = true;

        // Assert
        options.AssemblyLoaderOptions.DisableCascadeLoading.Should().BeTrue();
        options.AssemblyLoaderOptions.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void LoadOnNavigation_ShouldReturnFirstMatchForDuplicatePath()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();
        var path = "fetchdata";

        // Act
        options.LoadOnNavigation(path, "Assembly1.wasm");
        options.LoadOnNavigation(path, "Assembly2.wasm");

        // Assert - First match wins
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options) as AssemblyLoadConfiguration;
        config?.GetAssembly(path).Should().Be("Assembly1.wasm");
    }

    [Fact]
    public void LoadOnNavigation_ShouldHandleEmptyStrings()
    {
        // Arrange
        var options = new BlazyloadProviderOptions();

        // Act
        options.LoadOnNavigation("", "Assembly.wasm");
        options.LoadOnNavigation("path", "");

        // Assert
        var configProperty = typeof(BlazyloadProviderOptions).GetProperty("AssemblyLoadConfiguration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var config = configProperty?.GetValue(options) as AssemblyLoadConfiguration;
        config?.GetAssembly("").Should().Be("Assembly.wasm");
        config?.GetAssembly("path").Should().Be("");
    }
}

