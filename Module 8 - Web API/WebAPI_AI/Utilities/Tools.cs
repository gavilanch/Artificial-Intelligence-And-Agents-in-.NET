using Microsoft.Extensions.AI;
using WebAPI_AI.Services;

namespace WebAPI_AI.Utilities
{
    internal static class Tools
    {
        internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
        {
            var weatherService = sp.GetRequiredService<IWeatherService>();

            yield return AIFunctionFactory.Create(
                weatherService.GetWeather,
                new AIFunctionFactoryOptions
                {
                    Name = "get_weather",
                    Description = "Get the current weather of the given city"
                });

            var evaluateWeatherConditions = sp.GetRequiredService<EvaluateWeatherConditions>();

            yield return AIFunctionFactory.Create(
                evaluateWeatherConditions.EvaluateConditions,
                new AIFunctionFactoryOptions
                {
                    Name = "evaluate_weather_conditions",
                    Description = "Evaluates a weather condition (for example: “sunny,” “light rain,” “cloudy”) and determines whether it’s a good time to engage in outdoor activities."
                });

            var getEmailService = sp.GetRequiredService<FakeGetEmailService>();
            yield return AIFunctionFactory.Create(getEmailService.GetEmail);

            var sendEmailService = sp.GetRequiredService<FakeSendEmailService>();
            var sendEmailTool = AIFunctionFactory.Create(sendEmailService.SendEmail);
            yield return new ApprovalRequiredAIFunction(sendEmailTool);
        }
    }
}
