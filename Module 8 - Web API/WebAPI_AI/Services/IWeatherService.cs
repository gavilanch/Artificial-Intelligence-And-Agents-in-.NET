namespace WebAPI_AI.Services;

internal interface IWeatherService
{
    Task<string> GetWeather(string city);
}
