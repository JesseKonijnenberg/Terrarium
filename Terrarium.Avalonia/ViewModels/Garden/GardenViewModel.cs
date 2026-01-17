using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Models.Garden;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums.Garden;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Models;

namespace Terrarium.Avalonia.ViewModels;

public partial class GardenViewModel : ViewModelBase
{
    private readonly IGardenService _gardenService;
    private readonly IGardenEconomyService _economyService;
    private readonly Random _random = new();
    
    private const int PlantWidth = 140;
    private const int PlantHeight = 160;
    private const int GardenWidth = 1000;
    private const int GardenHeight = 600;

    public string Title => "Garden Sanctuary";

    [ObservableProperty]
    private int _waterBalance;

    public ObservableCollection<PlantUiModel> Plants { get; } = new();

    public GardenViewModel(IGardenService gardenService, IGardenEconomyService economyService)
    {
        _gardenService = gardenService;
        _economyService = economyService;

        WaterBalance = _economyService.WaterBalance;
        _economyService.BalanceChanged += UpdateBalance;

        LoadGarden();
    }

    private void UpdateBalance(object? sender, int newBalance) => WaterBalance = newBalance;

    private void LoadGarden()
    {
        Plants.Clear();
        var entities = _gardenService.GetGarden();

        // Starter seed if empty
        if (entities.Count == 0)
        {
            var starter = new PlantEntity { Id = "starter", Type = PlantType.Succulent };
            entities.Add(starter);
        }

        foreach (var entity in entities)
        {
            var (x, y) = FindValidLocation();
            Plants.Add(new PlantUiModel(entity, x, y));
        }
    }

    private (double x, double y) FindValidLocation()
    {
        for (int i = 0; i < 100; i++)
        {
            double x = _random.Next(20, GardenWidth - PlantWidth - 20);
            double y = _random.Next(20, GardenHeight - PlantHeight - 20);

            if (!IsOverlapping(x, y)) return (x, y);
        }

        return (0, 0);
    }

    private bool IsOverlapping(double x, double y)
    {
        return Plants.Any(plant => 
            x < (plant.X + PlantWidth) && (x + PlantWidth) > plant.X &&
            y < (plant.Y + PlantHeight) && (y + PlantHeight) > plant.Y);
    }

    [RelayCommand]
    private void WaterPlant(PlantUiModel? uiModel)
    {
        if (uiModel == null) return;

        const int cost = 5;
        if (_economyService.SpendWater(cost))
        {
            // Update persistence layer
            _gardenService.WaterPlant(uiModel.Entity, 20);

            // Trigger UI notification on the specific plant
            uiModel.Refresh();
        }
    }
}