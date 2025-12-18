using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Avalonia.ViewModels.Models;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia.ViewModels
{
    public class GardenViewModel : ViewModelBase
    {
        private readonly IGardenService _gardenService;
        private readonly IGardenEconomyService _economyService;
        private readonly Random _random = new Random();

        // CONSTANTS for Collision & Layout
        private const int PlantWidth = 140;  // Must match View
        private const int PlantHeight = 160; // Must match View
        private const int GardenWidth = 1000;
        private const int GardenHeight = 600;

        public string Title => "Garden Sanctuary";

        // WATER
        private int _waterBalance;
        public int WaterBalance
        {
            get => _waterBalance;
            set { _waterBalance = value; OnPropertyChanged(); }
        }

        // PLANTS (Using the UI Wrapper now)
        public ObservableCollection<PlantUiModel> Plants { get; } = new();

        public ICommand WaterPlantCommand { get; }

        public GardenViewModel(IGardenService gardenService, IGardenEconomyService economyService)
        {
            _gardenService = gardenService;
            _economyService = economyService;

            UpdateBalance(this, _economyService.WaterBalance);
            LoadGarden();

            _economyService.BalanceChanged += UpdateBalance;
            WaterPlantCommand = new RelayCommand(ExecuteWaterPlant);
        }

        private void UpdateBalance(object? sender, int newBalance) => WaterBalance = newBalance;

        private void LoadGarden()
        {
            Plants.Clear();
            var entities = _gardenService.GetGarden();

            // Starter seed if empty
            if (entities.Count == 0)
            {
                var starter = new PlantEntity { Id = "starter", Type = Terrarium.Core.Enums.PlantType.Succulent };
                entities.Add(starter);
            }

            foreach (var entity in entities)
            {
                var (x, y) = FindValidLocation();
                Plants.Add(new PlantUiModel(entity, x, y));
            }
        }

        // COLLISION ALGORITHM
        private (double x, double y) FindValidLocation()
        {
            int maxAttempts = 100;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate random pos within bounds (padded so they don't clip edge)
                double x = _random.Next(20, GardenWidth - PlantWidth - 20);
                double y = _random.Next(20, GardenHeight - PlantHeight - 20);

                if (!IsOverlapping(x, y))
                {
                    return (x, y);
                }
            }

            // Fallback: Just return 0,0 if garden is 100% full
            return (0, 0);
        }

        private bool IsOverlapping(double x, double y)
        {
            foreach (var plant in Plants)
            {
                // AABB Collision Check (Axis-Aligned Bounding Box)
                // We verify if the new rectangle intersects with any existing plant rectangle
                bool overlapX = x < (plant.X + PlantWidth) && (x + PlantWidth) > plant.X;
                bool overlapY = y < (plant.Y + PlantHeight) && (y + PlantHeight) > plant.Y;

                if (overlapX && overlapY) return true;
            }
            return false;
        }

        private void ExecuteWaterPlant(object? parameter)
        {
            // parameter is now the PlantUiModel (passed from the View)
            if (parameter is PlantUiModel uiModel)
            {
                int cost = 5;
                if (_economyService.SpendWater(cost))
                {
                    // 1. Logic Layer updates the Pure Entity
                    _gardenService.WaterPlant(uiModel.Entity, 20);

                    // 2. ViewModel Layer forces the UI notification
                    uiModel.Refresh();
                }
            }
        }
    }
}