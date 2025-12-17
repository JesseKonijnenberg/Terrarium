using System;
using Terrarium.Core.Interfaces;

namespace Terrarium.Logic.Services
{
    public class GardenEconomyService : IGardenEconomyService
    {
        // Singleton Instance
        private static GardenEconomyService? _instance;
        public static GardenEconomyService Instance => _instance ??= new GardenEconomyService();

        private int _waterBalance;

        // Event to tell the UI to update the counter
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