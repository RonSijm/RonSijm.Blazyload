using Microsoft.AspNetCore.Components;

namespace RonSijm.Demo.Blazyload.WeatherLib3.Pages;

public partial class FetchData
{
    [Inject]
    public IWeatherResolver WeatherResolver { get; set; }

    private WeatherForecast[] _forecasts;

    protected override async Task OnInitializedAsync()
    {
        _forecasts = await WeatherResolver.GetWeather();
    }
}