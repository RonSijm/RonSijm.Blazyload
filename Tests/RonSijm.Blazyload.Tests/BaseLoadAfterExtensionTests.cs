using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;
using System.Reflection;

namespace RonSijm.Blazyload.Tests;

public class BaseLoadAfterExtensionTests
{
    [Fact]
    public void SetReference_ShouldSetServiceProvider()
    {
        // Arrange
        var extension = new TestExtension();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());

        // Act
        extension.SetReference(serviceProvider);

        // Assert
        extension.ServiceProvider.Should().Be(serviceProvider);
    }

    [Fact]
    public void AssembliesLoaded_ShouldDoNothingByDefault()
    {
        // Arrange
        var extension = new TestExtension();
        var assemblies = new List<Assembly> { typeof(BaseLoadAfterExtensionTests).Assembly };

        // Act & Assert - Should not throw
        extension.AssembliesLoaded(assemblies);
    }

    [Fact]
    public void DescriptorsLoaded_ShouldDoNothingByDefault()
    {
        // Arrange
        var extension = new TestExtension();
        var descriptors = new List<ServiceDescriptor>
        {
            new ServiceDescriptor(typeof(string), "test")
        };

        // Act & Assert - Should not throw
        extension.DescriptorsLoaded(descriptors);
    }

    [Fact]
    public void AssembliesLoaded_CanBeOverridden()
    {
        // Arrange
        var extension = new CustomExtension();
        var assemblies = new List<Assembly> { typeof(BaseLoadAfterExtensionTests).Assembly };

        // Act
        extension.AssembliesLoaded(assemblies);

        // Assert
        extension.AssembliesLoadedCalled.Should().BeTrue();
        extension.LoadedAssemblies.Should().BeSameAs(assemblies);
    }

    [Fact]
    public void DescriptorsLoaded_CanBeOverridden()
    {
        // Arrange
        var extension = new CustomExtension();
        var descriptors = new List<ServiceDescriptor>
        {
            new ServiceDescriptor(typeof(string), "test")
        };

        // Act
        extension.DescriptorsLoaded(descriptors);

        // Assert
        extension.DescriptorsLoadedCalled.Should().BeTrue();
        extension.LoadedDescriptors.Should().BeSameAs(descriptors);
    }

    [Fact]
    public void SetReference_CanBeOverridden()
    {
        // Arrange
        var extension = new CustomExtension();
        var serviceProvider = new SyringeServiceProvider(new ServiceCollection());

        // Act
        extension.SetReference(serviceProvider);

        // Assert
        extension.SetReferenceCalled.Should().BeTrue();
        extension.ServiceProvider.Should().Be(serviceProvider);
    }

    [Fact]
    public void ServiceProvider_ShouldBeNullInitially()
    {
        // Arrange & Act
        var extension = new TestExtension();

        // Assert
        extension.ServiceProvider.Should().BeNull();
    }

    [Fact]
    public void AssembliesLoaded_ShouldHandleEmptyList()
    {
        // Arrange
        var extension = new TestExtension();
        var assemblies = new List<Assembly>();

        // Act & Assert - Should not throw
        extension.AssembliesLoaded(assemblies);
    }

    [Fact]
    public void DescriptorsLoaded_ShouldHandleEmptyList()
    {
        // Arrange
        var extension = new TestExtension();
        var descriptors = new List<ServiceDescriptor>();

        // Act & Assert - Should not throw
        extension.DescriptorsLoaded(descriptors);
    }

    private class TestExtension : BaseLoadAfterExtension
    {
    }

    private class CustomExtension : BaseLoadAfterExtension
    {
        public bool AssembliesLoadedCalled { get; private set; }
        public bool DescriptorsLoadedCalled { get; private set; }
        public bool SetReferenceCalled { get; private set; }
        public List<Assembly>? LoadedAssemblies { get; private set; }
        public List<ServiceDescriptor>? LoadedDescriptors { get; private set; }

        public override void AssembliesLoaded(List<Assembly> loadedAssemblies)
        {
            AssembliesLoadedCalled = true;
            LoadedAssemblies = loadedAssemblies;
        }

        public override void DescriptorsLoaded(List<ServiceDescriptor> loadedDescriptors)
        {
            DescriptorsLoadedCalled = true;
            LoadedDescriptors = loadedDescriptors;
        }

        public override void SetReference(SyringeServiceProvider serviceProvider)
        {
            SetReferenceCalled = true;
            base.SetReference(serviceProvider);
        }
    }
}

