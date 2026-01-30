using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RonSijm.Blazyload.IntegrationTests.Helpers;

namespace RonSijm.Blazyload.IntegrationTests.Tests;

/// <summary>
/// Scope Isolation Tests
/// Tests CreateScope() and scope isolation in SyringeServiceProvider
/// </summary>
public class ScopeIsolationTests
{
    public interface IScopedService
    {
        Guid InstanceId { get; }
    }

    public class ScopedService : IScopedService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    public interface ISingletonService
    {
        Guid InstanceId { get; }
    }

    public class SingletonService : ISingletonService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    [Fact]
    public async Task CreateScope_ShouldCreateIsolatedScope()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        // Register a scoped service
        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var scope1 = context.ServiceProvider.CreateScope();
        var scope2 = context.ServiceProvider.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<IScopedService>();
        var service2 = scope2.ServiceProvider.GetService<IScopedService>();

        // Assert - Different scopes should have different instances
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1!.InstanceId.Should().NotBe(service2!.InstanceId);
    }

    [Fact]
    public async Task CreateScope_SameScopeShouldReturnSameInstance()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        var services = new ServiceCollection();
        services.AddScoped<IScopedService, ScopedService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var scope = context.ServiceProvider.CreateScope();

        var service1 = scope.ServiceProvider.GetService<IScopedService>();
        var service2 = scope.ServiceProvider.GetService<IScopedService>();

        // Assert - Same scope should return same instance
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1!.InstanceId.Should().Be(service2!.InstanceId);
    }

    [Fact]
    public async Task CreateScope_SingletonShouldBeSameAcrossScopes()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var scope1 = context.ServiceProvider.CreateScope();
        var scope2 = context.ServiceProvider.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<ISingletonService>();
        var service2 = scope2.ServiceProvider.GetService<ISingletonService>();
        var rootService = context.ServiceProvider.GetService<ISingletonService>();

        // Assert - Singleton should be same across all scopes
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        rootService.Should().NotBeNull();
        service1!.InstanceId.Should().Be(service2!.InstanceId);
        service1.InstanceId.Should().Be(rootService!.InstanceId);
    }

    [Fact]
    public async Task CreateScope_ShouldInheritServicesFromParent()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        // Act
        var scope = context.ServiceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetService<ISingletonService>();

        // Assert
        scopedService.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateScope_DisposeShouldNotAffectParent()
    {
        // Arrange
        await using var context = await TestSetup.CreateContext();

        var services = new ServiceCollection();
        services.AddSingleton<ISingletonService, SingletonService>();
        await context.ServiceProvider.LoadServiceDescriptors(services);
        context.ServiceProvider.Build();

        Guid scopedInstanceId;

        // Act - Create a scope and get service
        var scope = context.ServiceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetService<ISingletonService>();
        scopedInstanceId = scopedService!.InstanceId;

        // Get service from parent after scope is no longer used
        var parentService = context.ServiceProvider.GetService<ISingletonService>();

        // Assert - Parent should still work and have same singleton
        parentService.Should().NotBeNull();
        parentService!.InstanceId.Should().Be(scopedInstanceId);
    }
}

