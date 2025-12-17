using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums;
using Terrarium.Core.Models;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class PlantUiModel : ViewModelBase
    {
        public PlantEntity Entity { get; }

        // --- WRAPPER PROPERTIES ---
        // We expose these so the View binds to the UiModel, not the Entity directly.

        public int GrowthProgress
        {
            get => Entity.GrowthProgress;
            // No setter: The logic layer updates the Entity, we just reflect it.
        }

        public PlantStage Stage
        {
            get => Entity.Stage;
        }

        public PlantType Type => Entity.Type; // Static, likely won't change

        // --- VISUAL COORDINATES ---
        private double _x;
        public double X
        {
            get => _x;
            set { _x = value; OnPropertyChanged(); }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set { _y = value; OnPropertyChanged(); }
        }

        // Z-Index ensures plants "lower" on screen appear "in front" (Isometric depth)
        public int ZIndex => (int)Y;

        public PlantUiModel(PlantEntity entity, double x, double y)
        {
            Entity = entity;
            X = x;
            Y = y;
        }

        // --- THE FIX ---
        // Call this method after the Service modifies the Entity.
        // It tells the View: "Check these values again!"
        public void Refresh()
        {
            OnPropertyChanged(nameof(GrowthProgress));
            OnPropertyChanged(nameof(Stage));
        }
    }
}