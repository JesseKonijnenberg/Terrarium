using System;
using System.Collections.Generic;
using System.Text;
using Terrarium.Core.Models;

namespace Terrarium.Core.Interfaces
{
    public interface IGardenService
    {
        List<PlantEntity> GetGarden();
        void WaterPlant(PlantEntity plant, int amount);
    }
}
