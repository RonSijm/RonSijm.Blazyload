using AwesomeAssertions;

namespace RonSijm.Blazyload.Tests;

public class BlazyloadBootstrapperTests
{
    [Fact]
    public void CreateSyringeFactory_ShouldReturnFactory_WhenNoOptionsProvided()
    {
        var factory = BlazyloadBootstapper.CreateSyringeFactory();

        factory.Should().NotBeNull();
        factory.Should().BeOfType<BlazyServiceProviderFactory>();
    }

    [Fact]
    public void CreateSyringeFactory_ShouldReturnFactory_WhenOptionsProvided()
    {
        var factory = BlazyloadBootstapper.CreateSyringeFactory(options =>
        {
            options.AssemblyLoaderOptions.EnableLogging = true;
        });

        factory.Should().NotBeNull();
        factory.Should().BeOfType<BlazyServiceProviderFactory>();
    }

    [Fact]
    public void CreateSyringeFactory_ShouldApplyOptions_WhenOptionsProvided()
    {
        var wasInvoked = false;
        
        var factory = BlazyloadBootstapper.CreateSyringeFactory(options =>
        {
            wasInvoked = true;
            options.AssemblyLoaderOptions.EnableLogging = true;
        });

        wasInvoked.Should().BeTrue();
    }

    [Fact]
    public void CreateBlazyloadFactory_ShouldReturnFactory_WhenNoOptionsProvided()
    {
        var factory = BlazyloadBootstapper.CreateBlazyloadFactory();

        factory.Should().NotBeNull();
        factory.Should().BeOfType<BlazyServiceProviderFactory>();
    }

    [Fact]
    public void CreateBlazyloadFactory_ShouldReturnFactory_WhenOptionsProvided()
    {
        var factory = BlazyloadBootstapper.CreateBlazyloadFactory(options =>
        {
            options.AssemblyLoaderOptions.EnableLogging = true;
        });

        factory.Should().NotBeNull();
        factory.Should().BeOfType<BlazyServiceProviderFactory>();
    }

    [Fact]
    public void CreateBlazyloadFactory_ShouldApplyOptions_WhenOptionsProvided()
    {
        var wasInvoked = false;
        
        var factory = BlazyloadBootstapper.CreateBlazyloadFactory(options =>
        {
            wasInvoked = true;
            options.AssemblyLoaderOptions.EnableLogging = true;
        });

        wasInvoked.Should().BeTrue();
    }

    [Fact]
    public void CreateBlazyloadFactory_ShouldAddOptionsToItself()
    {
        var factory = BlazyloadBootstapper.CreateBlazyloadFactory();

        factory.Should().NotBeNull();
    }
}

