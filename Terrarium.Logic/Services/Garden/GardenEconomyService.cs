using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Services;
using Terrarium.Core.Messages;

namespace Terrarium.Core.Interfaces.Update;

public class GardenEconomyService : IGardenEconomyService
{
    private readonly IBoardRepository _boardRepository;
    private readonly ITerrariumEventBus _eventBus;
    private int _waterBalance = 50;

    public event EventHandler<int>? BalanceChanged;

    public int WaterBalance => _waterBalance;

    public GardenEconomyService(IBoardRepository boardRepository, ITerrariumEventBus eventBus)
    {
        _boardRepository = boardRepository;
        _eventBus = eventBus;
        
        _eventBus.Subscribe<TaskMovedMessage>(OnTaskMoved);
    }

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

    private void OnTaskMoved(TaskMovedMessage message)
    {
        if (message.NewColumnTitle.Equals("Complete", StringComparison.OrdinalIgnoreCase) || 
            message.NewColumnTitle.Equals("Done", StringComparison.OrdinalIgnoreCase))
        {
            EarnWater(20);
        }
    }
}