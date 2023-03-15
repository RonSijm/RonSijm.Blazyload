namespace RonSijm.Demo.Blazyload.WeatherLib1;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}