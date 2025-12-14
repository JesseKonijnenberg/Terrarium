namespace Terrarium.Core.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherReport> GetCurrentWeatherAsync();
    }

    public record WeatherReport(bool IsSunny, bool IsRaining, double Temperature);
}
