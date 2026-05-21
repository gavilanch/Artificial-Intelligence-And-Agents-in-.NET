using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot.Services
{
    internal class FakeWeatherService : IWeatherService
    {
        public async Task<string> GetWeather(string city)
        {
            return city.ToLower() switch
            {
                "santo domingo" => "Sunny, 32°C",
                "madrid" => "Cloudy, 18°C",
                "new york" => "Light rain, 12°C",
                _ => "I don't have weather information for that city"
            };
        }
    }
}
