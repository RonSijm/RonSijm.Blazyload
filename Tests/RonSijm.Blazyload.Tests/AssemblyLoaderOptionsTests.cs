using AwesomeAssertions;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.Tests;

public class AssemblyLoaderOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new AssemblyLoaderOptions();

        // Assert
        options.DisableCascadeLoading.Should().BeFalse();
        options.EnableLoggingForCascadeErrors.Should().BeFalse();
        options.EnableLogging.Should().BeFalse();
        options.AfterLoadAssembliesExtensions.Should().NotBeNull();
        options.AfterLoadAssembliesExtensions.Should().BeEmpty();
    }

    [Fact]
    public void DisableCascadeLoading_ShouldBeSettable()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();

        // Act
        options.DisableCascadeLoading = true;

        // Assert
        options.DisableCascadeLoading.Should().BeTrue();
    }

    [Fact]
    public void EnableLoggingForCascadeErrors_ShouldBeSettable()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();

        // Act
        options.EnableLoggingForCascadeErrors = true;

        // Assert
        options.EnableLoggingForCascadeErrors.Should().BeTrue();
    }

    [Fact]
    public void EnableLogging_ShouldBeSettable()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();

        // Act
        options.EnableLogging = true;

        // Assert
        options.EnableLogging.Should().BeTrue();
    }

    [Fact]
    public void AfterLoadAssembliesExtensions_ShouldAllowAddingExtensions()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var extension = new TestLoadAfterExtension();

        // Act
        options.AfterLoadAssembliesExtensions.Add(extension);

        // Assert
        options.AfterLoadAssembliesExtensions.Should().HaveCount(1);
        options.AfterLoadAssembliesExtensions[0].Should().Be(extension);
    }

    [Fact]
    public void AfterLoadAssembliesExtensions_ShouldAllowMultipleExtensions()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var extension1 = new TestLoadAfterExtension();
        var extension2 = new TestLoadAfterExtension();

        // Act
        options.AfterLoadAssembliesExtensions.Add(extension1);
        options.AfterLoadAssembliesExtensions.Add(extension2);

        // Assert
        options.AfterLoadAssembliesExtensions.Should().HaveCount(2);
    }

    [Fact]
    public void AfterLoadAssembliesExtensions_ShouldAllowRemovingExtensions()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        var extension = new TestLoadAfterExtension();
        options.AfterLoadAssembliesExtensions.Add(extension);

        // Act
        options.AfterLoadAssembliesExtensions.Remove(extension);

        // Assert
        options.AfterLoadAssembliesExtensions.Should().BeEmpty();
    }

    [Fact]
    public void AfterLoadAssembliesExtensions_ShouldAllowClearing()
    {
        // Arrange
        var options = new AssemblyLoaderOptions();
        options.AfterLoadAssembliesExtensions.Add(new TestLoadAfterExtension());
        options.AfterLoadAssembliesExtensions.Add(new TestLoadAfterExtension());

        // Act
        options.AfterLoadAssembliesExtensions.Clear();

        // Assert
        options.AfterLoadAssembliesExtensions.Should().BeEmpty();
    }

    private class TestLoadAfterExtension : ILoadAfterExtension
    {
        public void AssembliesLoaded(List<System.Reflection.Assembly> loadedAssemblies)
        {
        }

        public void DescriptorsLoaded(List<Microsoft.Extensions.DependencyInjection.ServiceDescriptor> loadedDescriptors)
        {
        }

        public void SetReference(RonSijm.Syringe.SyringeServiceProvider serviceProvider)
        {
        }
    }
}

