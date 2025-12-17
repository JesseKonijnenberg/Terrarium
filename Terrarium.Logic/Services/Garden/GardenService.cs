using System.Collections.Generic;
using Terrarium.Core.Enums;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;

namespace Terrarium.Logic.Services
{
    public class GardenService : IGardenService
    {
        public List<PlantEntity> GetGarden()
        {
            return new List<PlantEntity>
            {
                new PlantEntity { Id = "1", Type = PlantType.Succulent, Stage = PlantStage.Growing, GrowthProgress = 40, OwnerName = "Jesse" },
                new PlantEntity { Id = "2", Type = PlantType.Fern, Stage = PlantStage.Sprout, GrowthProgress = 10, OwnerName = "Team" },
                new PlantEntity { Id = "3", Type = PlantType.Monstera, Stage = PlantStage.Seed, GrowthProgress = 0, OwnerName = "Alice" }
            };
        }

        public void WaterPlant(PlantEntity plant, int amount)
        {
            plant.GrowthProgress += amount;
            if (plant.GrowthProgress >= 100)
            {
                plant.GrowthProgress = 0;
                if (plant.Stage != PlantStage.Blooming)
                {
                    plant.Stage++;
                }
            }
        }
    }
}