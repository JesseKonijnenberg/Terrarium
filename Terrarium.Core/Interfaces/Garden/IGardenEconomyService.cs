namespace Terrarium.Core.Interfaces.Garden
{
    public interface IGardenEconomyService
    {
        int WaterBalance { get; }
        event EventHandler<int>? BalanceChanged;
        void EarnWater(int amount);
        bool SpendWater(int amount);
    }
}
