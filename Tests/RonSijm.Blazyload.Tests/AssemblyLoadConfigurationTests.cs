using AwesomeAssertions;

namespace RonSijm.Blazyload.Tests;

public class AssemblyLoadConfigurationTests
{
    [Fact]
    public void Add_ShouldAddPathAndAssemblyMapping()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        var path = "fetchdata1";
        var assembly = "MyAssembly.wasm";

        // Act
        config.Add(path, assembly);

        // Assert
        var result = config.GetAssembly(path);
        result.Should().Be(assembly);
    }

    [Fact]
    public void Add_ShouldReturnFirstMatchForDuplicatePath()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        var path = "fetchdata1";
        var assembly1 = "Assembly1.wasm";
        var assembly2 = "Assembly2.wasm";

        // Act
        config.Add(path, assembly1);
        config.Add(path, assembly2);

        // Assert - First match wins
        var result = config.GetAssembly(path);
        result.Should().Be(assembly1);
    }

    [Fact]
    public void GetAssembly_ShouldReturnNullForNonExistentPath()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act
        var result = config.GetAssembly("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAssembly_ShouldReturnCorrectAssemblyForPath()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        config.Add("path1", "Assembly1.wasm");
        config.Add("path2", "Assembly2.wasm");

        // Act
        var result1 = config.GetAssembly("path1");
        var result2 = config.GetAssembly("path2");

        // Assert
        result1.Should().Be("Assembly1.wasm");
        result2.Should().Be("Assembly2.wasm");
    }

    [Fact]
    public void Remove_ShouldRemoveAllPathsForAssembly()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        var assembly = "MyAssembly.wasm";
        config.Add("path1", assembly);
        config.Add("path2", assembly);
        config.Add("path3", "OtherAssembly.wasm");

        // Act
        config.Remove(assembly);

        // Assert
        config.GetAssembly("path1").Should().BeNull();
        config.GetAssembly("path2").Should().BeNull();
        config.GetAssembly("path3").Should().Be("OtherAssembly.wasm");
    }

    [Fact]
    public void Remove_ShouldHandleNonExistentAssembly()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        config.Add("path1", "Assembly1.wasm");

        // Act
        config.Remove("NonExistent.wasm");

        // Assert
        config.GetAssembly("path1").Should().Be("Assembly1.wasm");
    }

    [Fact]
    public void Remove_ShouldHandleEmptyConfiguration()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act & Assert - Should not throw
        config.Remove("SomeAssembly.wasm");
    }

    [Fact]
    public void Add_ShouldHandleMultipleDifferentPaths()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act
        for (int i = 0; i < 100; i++)
        {
            config.Add($"path{i}", $"Assembly{i}.wasm");
        }

        // Assert
        for (int i = 0; i < 100; i++)
        {
            config.GetAssembly($"path{i}").Should().Be($"Assembly{i}.wasm");
        }
    }

    [Fact]
    public void Add_ShouldHandleEmptyStrings()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act
        config.Add("", "Assembly.wasm");
        config.Add("path", "");

        // Assert
        config.GetAssembly("").Should().Be("Assembly.wasm");
        config.GetAssembly("path").Should().Be("");
    }

    [Fact]
    public void AddWithCriteria_ShouldMatchPathsBasedOnLambda()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add a criteria that matches any path starting with "weather"
        config.Add(path => path.StartsWith("weather"), "WeatherAssembly.wasm");

        // Assert
        config.GetAssembly("weather").Should().Be("WeatherAssembly.wasm");
        config.GetAssembly("weather/forecast").Should().Be("WeatherAssembly.wasm");
        config.GetAssembly("weatherdata").Should().Be("WeatherAssembly.wasm");
        config.GetAssembly("other").Should().BeNull();
    }

    [Fact]
    public void AddWithCriteria_ShouldMatchPathsContainingSubstring()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add a criteria that matches any path containing "admin"
        config.Add(path => path.Contains("admin"), "AdminAssembly.wasm");

        // Assert
        config.GetAssembly("admin").Should().Be("AdminAssembly.wasm");
        config.GetAssembly("user/admin/settings").Should().Be("AdminAssembly.wasm");
        config.GetAssembly("superadmin").Should().Be("AdminAssembly.wasm");
        config.GetAssembly("user/settings").Should().BeNull();
    }

    [Fact]
    public void AddWithCriteria_ShouldSupportMultipleCriteriaForSameAssembly()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add multiple criteria that map to the same assembly
        config.Add(path => path.StartsWith("weather"), "SharedAssembly.wasm");
        config.Add(path => path.StartsWith("forecast"), "SharedAssembly.wasm");

        // Assert
        config.GetAssembly("weather/today").Should().Be("SharedAssembly.wasm");
        config.GetAssembly("forecast/weekly").Should().Be("SharedAssembly.wasm");
        config.GetAssembly("other").Should().BeNull();
    }

    [Fact]
    public void AddWithCriteria_FirstMatchWins()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add overlapping criteria
        config.Add(path => path.StartsWith("weather"), "WeatherAssembly.wasm");
        config.Add(path => path.Contains("weather"), "GenericWeatherAssembly.wasm");

        // Assert - First match wins
        config.GetAssembly("weather/forecast").Should().Be("WeatherAssembly.wasm");
        config.GetAssembly("bad-weather").Should().Be("GenericWeatherAssembly.wasm");
    }

    [Fact]
    public void MixedPathAndCriteria_ShouldWorkTogether()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Mix exact path and criteria-based mappings
        config.Add("exact-path", "ExactAssembly.wasm");
        config.Add(path => path.StartsWith("prefix"), "PrefixAssembly.wasm");

        // Assert
        config.GetAssembly("exact-path").Should().Be("ExactAssembly.wasm");
        config.GetAssembly("prefix/something").Should().Be("PrefixAssembly.wasm");
        config.GetAssembly("other").Should().BeNull();
    }

    [Fact]
    public void MixedPathAndCriteria_ExactPathAddedFirstTakesPrecedence()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add exact path first, then criteria that would also match
        config.Add("weather", "ExactWeatherAssembly.wasm");
        config.Add(path => path.StartsWith("weather"), "CriteriaWeatherAssembly.wasm");

        // Assert - Exact path was added first, so it wins
        config.GetAssembly("weather").Should().Be("ExactWeatherAssembly.wasm");
        config.GetAssembly("weather/forecast").Should().Be("CriteriaWeatherAssembly.wasm");
    }

    [Fact]
    public void MixedPathAndCriteria_CriteriaAddedFirstTakesPrecedence()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add criteria first, then exact path
        config.Add(path => path.StartsWith("weather"), "CriteriaWeatherAssembly.wasm");
        config.Add("weather", "ExactWeatherAssembly.wasm");

        // Assert - Criteria was added first, so it wins for "weather"
        config.GetAssembly("weather").Should().Be("CriteriaWeatherAssembly.wasm");
        config.GetAssembly("weather/forecast").Should().Be("CriteriaWeatherAssembly.wasm");
    }

    [Fact]
    public void Remove_ShouldRemoveCriteriaBasedMappings()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();
        config.Add(path => path.StartsWith("weather"), "WeatherAssembly.wasm");
        config.Add(path => path.StartsWith("admin"), "AdminAssembly.wasm");

        // Act
        config.Remove("WeatherAssembly.wasm");

        // Assert
        config.GetAssembly("weather/forecast").Should().BeNull();
        config.GetAssembly("admin/settings").Should().Be("AdminAssembly.wasm");
    }

    [Fact]
    public void AddWithCriteria_ShouldSupportComplexCriteria()
    {
        // Arrange
        var config = new AssemblyLoadConfiguration();

        // Act - Add a complex criteria using regex-like pattern
        config.Add(path => path.EndsWith("/edit") || path.EndsWith("/create"), "EditorAssembly.wasm");

        // Assert
        config.GetAssembly("user/edit").Should().Be("EditorAssembly.wasm");
        config.GetAssembly("product/create").Should().Be("EditorAssembly.wasm");
        config.GetAssembly("user/view").Should().BeNull();
        config.GetAssembly("edit/user").Should().BeNull();
    }
}

