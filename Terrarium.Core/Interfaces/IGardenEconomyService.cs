using System;
using System.Collections.Generic;
using System.Text;

namespace Terrarium.Core.Interfaces
{
    public interface IGardenEconomyService
    {
        int WaterBalance { get; }
        event EventHandler<int>? BalanceChanged;
        void EarnWater(int amount);
        bool SpendWater(int amount);
    }
}
