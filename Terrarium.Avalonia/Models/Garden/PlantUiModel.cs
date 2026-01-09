using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums.Garden;
using Terrarium.Core.Models;

namespace Terrarium.Avalonia.Models.Garden;

public partial class PlantUiModel : ViewModelBase
{
    public PlantEntity Entity { get; }

    // Visual coordinates with automatic notification
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZIndex))]
    private double _x;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZIndex))]
    private double _y;

    // Isometric depth: higher Y values (lower on screen) have higher Z-Index
    public int ZIndex => (int)Y;

    // These properties wrap the underlying Entity
    public int GrowthProgress => Entity.GrowthProgress;
    public PlantStage Stage => Entity.Stage;
    public PlantType Type => Entity.Type;

    public PlantUiModel(PlantEntity entity, double x, double y)
    {
        Entity = entity;
        _x = x;
        _y = y;
    }

    /// <summary>
    /// Forces the UI to re-read values from the underlying Entity.
    /// Useful when the logic layer (GardenService) updates the growth state.
    /// </summary>
    public void Refresh()
    {
        OnPropertyChanged(nameof(GrowthProgress));
        OnPropertyChanged(nameof(Stage));
    }
}