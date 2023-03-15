namespace RonSijm.Demo.Blazyload.WeatherLib3;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}