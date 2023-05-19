namespace RonSijm.Demo.Blazyload.CustomPathing.Host.Auth;

public class DummyAuthHandler
{
    public bool HandleAuth(string assembly, HttpRequestMessage httpMessage, BlazyAssemblyOptions assemblyOptions)
    {
        if (assembly == "RonSijm.Demo.Blazyload.WeatherLib2.dll")
        {
            httpMessage.Headers.Add("authorization", "Bearer eyJhb-mock-auth-header");
            if (httpMessage.RequestUri != null)
            {
                httpMessage.RequestUri = new Uri(httpMessage.RequestUri.OriginalString + "?authorization=123");
            }
        }

        return true;
    }
}