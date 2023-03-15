namespace RonSijm.Demo.Blazyload.WeatherLib2;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}