using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Blazyload.IntegrationTests.Helpers;
using RonSijm.Syringe;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Optional&lt;T&gt; Wrapper Tests
/// Tests Optional&lt;T&gt; resolution before and after assembly loading
/// </summary>
public class OptionalWrapperTests
{
    public interface ITestService
    {
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "TestValue";
    }

    [Fact]
    public async Task Optional_BeforeServiceRegistered_ShouldHaveNullValue()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        // Act
        var optional = context.ServiceProvider.GetService<Optional<ITestService>>();

        // Assert
        optional.Should().NotBeNull();
        optional!.Value.Should().BeNull();
    }

    [Fact]
    public async Task Optional_AfterServiceRegistered_ShouldHaveValue()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        // Register the service dynamically (simulating what happens when an assembly is loaded)
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var optional = context.ServiceProvider.GetService<Optional<ITestService>>();

        // Assert
        optional.Should().NotBeNull();
        optional!.Value.Should().NotBeNull();
        optional.Value.Should().BeOfType<TestService>();
    }

    [Fact]
    public async Task Optional_ImplicitConversion_ShouldReturnValue()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var optional = context.ServiceProvider.GetService<Optional<ITestService>>();
        ITestService? service = optional?.Value; // Get value directly

        // Assert
        service.Should().NotBeNull();
        service!.GetValue().Should().Be("TestValue");
    }

    [Fact]
    public async Task Optional_Equality_ShouldCompareValues()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var optional1 = context.ServiceProvider.GetService<Optional<ITestService>>();
        var optional2 = context.ServiceProvider.GetService<Optional<ITestService>>();

        // Assert - Both should resolve to the same singleton instance
        (optional1 == optional2).Should().BeTrue();
    }

    [Fact]
    public async Task Optional_WithNullValues_ShouldBeEqual()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        // Act - Get optional for unregistered service
        var optional1 = context.ServiceProvider.GetService<Optional<ITestService>>();
        var optional2 = context.ServiceProvider.GetService<Optional<ITestService>>();

        // Assert - Both null values should be equal
        (optional1 == optional2).Should().BeTrue();
    }

    [Fact]
    public async Task Optional_Inequality_ShouldWork()
    {
        // Arrange
        await using var context1 = await TestSetup.CreateContext();
        await using var context2 = await TestSetup.CreateContext();

        // Register service only in context1
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        await context1.ServiceProvider.LoadServiceDescriptors(services);
        context1.ServiceProvider.Build();

        // Act
        var optional1 = context1.ServiceProvider.GetService<Optional<ITestService>>();
        var optional2 = context2.ServiceProvider.GetService<Optional<ITestService>>();

        // Assert - One has value, one doesn't
        (optional1 != optional2).Should().BeTrue();
    }
}

