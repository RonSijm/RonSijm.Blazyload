using System.Net.Http.Json;

namespace RonSijm.Demo.Blazyload.WeatherLib4.Component;

public class WeatherResolver(HttpClient client) : IWeatherResolver
{
    public async Task<WeatherForecast[]> GetWeather()
    {
        return await client.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }
}