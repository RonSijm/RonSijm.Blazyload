using System.Net.Http.Json;

namespace RonSijm.Demo.Blazyload.WeatherLib1;

public class WeatherResolver : IWeatherResolver
{
    private readonly HttpClient _client;

    public WeatherResolver(HttpClient client)
    {
        _client = client;
    }

    public async Task<WeatherForecast[]> GetWeather()
    {
        return await _client.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }
}