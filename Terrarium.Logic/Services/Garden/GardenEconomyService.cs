using Terrarium.Core.Interfaces.Garden;

namespace Terrarium.Core.Interfaces.Update
{
    public class GardenEconomyService : IGardenEconomyService
    {
        private int _waterBalance = 50;

        public event EventHandler<int>? BalanceChanged;

        public int WaterBalance => _waterBalance;

        public void EarnWater(int amount)
        {
            _waterBalance += amount;
            BalanceChanged?.Invoke(this, _waterBalance);
        }

        public bool SpendWater(int amount)
        {
            if (_waterBalance >= amount)
            {
                _waterBalance -= amount;
                BalanceChanged?.Invoke(this, _waterBalance);
                return true;
            }
            return false;
        }
    }
}