namespace RonSijm.Demo.Blazyload.WeatherLibOptionalNotNull;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}