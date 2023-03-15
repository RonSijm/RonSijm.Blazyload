namespace RonSijm.Demo.Blazyload.WeatherLib4.Component;

public interface IWeatherResolver
{
    Task<WeatherForecast[]> GetWeather();
}