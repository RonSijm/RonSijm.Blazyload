using System.Net.Http.Json;

namespace RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull;

public class WeatherResolver(HttpClient client) : IWeatherResolver
{
    public async Task<WeatherForecast[]> GetWeather()
    {
        return await client.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
    }
}