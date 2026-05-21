using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot.Services
{
    internal interface IWeatherService
    {
        Task<string> GetWeather(string city);
    }
}
