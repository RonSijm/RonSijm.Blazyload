using RonSijm.Syringe;

namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;

public class DummyAuthHandler
{
    public bool HandleAuth(string assembly, HttpRequestMessage httpMessage, AssemblyOptions _)
    {
        if (assembly != "RonSijm.Demo.Blazyload.WeatherLib2.wasm")
        {
            return true;
        }

        httpMessage.Headers.Add("authorization", "Bearer eyJhb-mock-auth-header");

        if (httpMessage.RequestUri != null)
        {
            httpMessage.RequestUri = new Uri(httpMessage.RequestUri.OriginalString + "?authorization=123");
        }

        return true;
    }
}