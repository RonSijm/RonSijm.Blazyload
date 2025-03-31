using Microsoft.AspNetCore.Components;
using RonSijm.Blazyload;
using RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;
using RonSijm.Demo.Blazyload.CustomPathing.Host.Models;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Pages;

public class AWSAuthComponent : ComponentBase
{
    [Inject] public AWSAuthHandler AWSAuthHandler { get; set; }
    [Inject] public BlazyAssemblyLoader BlazyAssemblyLoader { get; set; }

    protected readonly AWSDataModel Model = new();
    protected string ButtonColor = "btn-primary";

    protected void HandleSubmit()
    {
        if (Model != null)
        {
            AWSAuthHandler.SetCredentials(Model.BucketUrl, Model.AccessKey, Model.SecretKey);
            ButtonColor = "btn-success";
        }
    }

    protected async Task HandleSubmitAndLoad()
    {
        if (Model != null)
        {
            AWSAuthHandler.SetCredentials(Model.BucketUrl, Model.AccessKey, Model.SecretKey);
            await BlazyAssemblyLoader.LoadAssemblyAsync("RonSijm.Demo.Blazyload.WeatherLib3.wasm");
            ButtonColor = "btn-success";
        }
    }
}