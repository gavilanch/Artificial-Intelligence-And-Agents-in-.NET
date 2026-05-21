namespace BlazorAI.Services;

internal class FakeWeatherService : IWeatherService
{
    public Task<string> GetWeather(string city)
    {
        var weather = city.ToLower() switch
        {
            "santo domingo" => "Sunny, 32°C",
            "madrid" => "Cloudy, 18°C",
            "new york" => "Light rain, 12°C",
            _ => "I do not have weather information for that city."
        };

        return Task.FromResult(weather);
    }
}
