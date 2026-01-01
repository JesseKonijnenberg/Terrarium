using Terrarium.Core.Enums.Garden;

namespace Terrarium.Core.Models
{
    public class PlantEntity
    {
        public string Id { get; set; } = string.Empty;
        public PlantType Type { get; set; }
        public PlantStage Stage { get; set; } = PlantStage.Seed;
        public int GrowthProgress { get; set; } = 0;
        public string OwnerName { get; set; } = "You";
    }
}