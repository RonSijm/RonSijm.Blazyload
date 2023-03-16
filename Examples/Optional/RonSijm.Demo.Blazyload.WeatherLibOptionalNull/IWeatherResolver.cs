namespace RonSijm.Demo.Blazyload.WeatherLibOptionalNull;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}