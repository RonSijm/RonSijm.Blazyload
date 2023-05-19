using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RonSijm.Blazyload.Features.DIComponents;
using RonSijm.Blazyload.Features.Options.Models;
using RonSijm.Demo.Blazyload.Host.Client;

namespace RonSijm.Demo.Blazyload.Host.Tests;

public class ClickAllMenuItemsTest : TestContext
{
    [Fact]
    public async Task ClickAllMenuItems()
    {
        // TODO: Probably name a bUnit ticket to see if any of this is possible
        // https://github.com/dotnet/aspnetcore/issues/24815

        // If bUnit doesn't work I might have to playwright this

        var options = new BlazyOptions();
        Program.BlazyConfig(options);

        var serviceProviderFactory = new BlazyServiceProviderFactory(options, Services);
        var serviceBuilder = serviceProviderFactory.CreateBuilder(Services);
        var serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceBuilder);

        Services.Add(new ServiceDescriptor(typeof(BlazyServiceProvider), serviceProvider));
        Services.AddFallbackServiceProvider(serviceProvider);

        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/fetchdata1");

        var cut = RenderComponent<App>();

        var menuItems = cut.FindAll(".nav-item").ToList();

        // Click every menu item twice to start lazy loading,
        // And ensuring double-loading works
        foreach (var menuItem in menuItems)
        {
            var link = menuItem.Children.First();
            await link.ClickAsync(new MouseEventArgs());
            cut.Render();
        }

        foreach (var menuItem in menuItems)
        {
            var link = menuItem.Children.First();
            await link.ClickAsync(new MouseEventArgs());
            cut.Render();
        }
    }
}