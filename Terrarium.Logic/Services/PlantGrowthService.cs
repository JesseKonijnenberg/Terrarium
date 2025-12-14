using Terrarium.Core.Models;
using Terrarium.Core.Interfaces;

namespace Terrarium.Logic.Services
{
    public class PlantGrowthService
    {
        public void ApplyWeatherEffects(Plant plant, WeatherReport weather)
        {
            double evaporation = 0;

            if (weather.IsSunny)
            {
                evaporation = 2.0;
            }
            else if (weather.IsRaining)
            {
                evaporation = 0.0;
            }
            else
            {
                evaporation = 0.5;
            }

            double targetHydration = plant.Hydration - evaporation;
            plant.Hydration = Math.Clamp(targetHydration, 0, 100);

            double currentSun = weather.IsSunny ? 100.0 : 20.0;
            plant.Sunlight = currentSun;

            UpdatePlantState(plant);
        }

        private void UpdatePlantState(Plant plant)
        {
            if (plant.Hydration < 40)
            {
                plant.State = PlantState.Thirsty;
            }
            else if (plant.Hydration > 65 && plant.Sunlight < 30)
            {
                // Too much water, not enough sun
                plant.State = PlantState.Wilting;
            }
            else
            {
                plant.State = PlantState.Happy;
            }
        }
    }
}