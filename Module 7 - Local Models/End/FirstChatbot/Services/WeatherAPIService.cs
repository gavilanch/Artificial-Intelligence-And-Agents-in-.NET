using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace FirstChatbot.Services
{
    internal class WeatherAPIService(HttpClient httpClient) : IWeatherService
    {
        public async Task<string> GetWeather(string city)
        {
            var apiKey = Environment.GetEnvironmentVariable("WEATHER_API_KEY");
            var cityURL = Uri.EscapeDataString(city);
            var url = $"http://api.weatherapi.com/v1/current.json?key={apiKey}&q={cityURL}&aqi=no";
            var weatherResponse = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
            return weatherResponse!.Current.Condition.Text;
        }

        public class WeatherResponse
        {
            [JsonPropertyName("current")]
            public Current Current { get; set; } = default!;
        }

        public class Current
        {
            [JsonPropertyName("condition")]
            public Condition Condition { get; set; } = default!;
        }

        public class Condition
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = default!;
        }

    }
}
