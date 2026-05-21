using System;
using System.Collections.Generic;
using System.Text;

namespace FirstChatbot.Services
{
    internal class EvaluateWeatherConditions
    {
        public string EvaluateConditions(string weatherCondition)
        {
            weatherCondition = weatherCondition.ToLower();

            // Rain / Drizzle / Precipitation
            if (weatherCondition.Contains("rain") ||
                weatherCondition.Contains("drizzle") ||
                weatherCondition.Contains("precipitation"))
                return "It's not a good time for outdoor activities";

            // Storms
            if (weatherCondition.Contains("storm") ||
                weatherCondition.Contains("stormy"))
                return "Avoid going outside, dangerous weather conditions";

            // Snow / Snowfall / Blizzard
            if (weatherCondition.Contains("snow") ||
                weatherCondition.Contains("snowfall") ||
                weatherCondition.Contains("blizzard"))
                return "Cold and potentially dangerous conditions, go out only if necessary";

            // Mist / Fog
            if (weatherCondition.Contains("mist") ||
                weatherCondition.Contains("fog"))
                return "Be cautious when going out, visibility may be reduced";

            // Sunny
            if (weatherCondition.Contains("sunny"))
                return "Great weather to go outside";

            // Cloudy
            if (weatherCondition.Contains("cloudy"))
                return "You can go outside, but it's not ideal weather";

            return "Normal conditions";
        }
    }
}
