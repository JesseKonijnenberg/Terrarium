using Terrarium.Core.Interfaces;

namespace Terrarium.Logic.Services
{
    public class LocalWeatherService : IWeatherService
    {
        private readonly Random _random = new();

        public async Task<WeatherReport> GetCurrentWeatherAsync()
        {
            await Task.Delay(100);

            int roll = _random.Next(0, 101);

            bool isSunny = roll <= 60;
            bool isRaining = roll > 90;

            double temp = isSunny ? 25.0 : (isRaining ? 15.0 : 20.0); // If it's sunny, it's hot. If it's raining, it's cool.

            return new WeatherReport(isSunny, isRaining, temp);
        }
    }
}
