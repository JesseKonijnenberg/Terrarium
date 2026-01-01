using Terrarium.Core.Models;

namespace Terrarium.Core.Interfaces.Garden
{
    public interface IGardenService
    {
        List<PlantEntity> GetGarden();
        void WaterPlant(PlantEntity plant, int amount);
    }
}
