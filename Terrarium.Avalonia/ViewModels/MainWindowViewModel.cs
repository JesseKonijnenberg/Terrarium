using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

/// <summary>
/// The main shell of the application, managing top-level navigation between views.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    public KanbanBoardViewModel BoardVm { get; }
    public GardenViewModel GardenVm { get; }
    public SettingsViewModel SettingsVm { get; }

    /// <summary>
    /// The current view being displayed in the main content area.
    /// The toolkit generates the 'CurrentPage' property automatically.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentPage;

    public MainWindowViewModel(
        KanbanBoardViewModel boardVm,
        GardenViewModel gardenVm,
        SettingsViewModel settingsVm)
    {
        BoardVm = boardVm;
        GardenVm = gardenVm;
        SettingsVm = settingsVm;
        
        _currentPage = BoardVm; // Default starting page
    }

    [RelayCommand]
    private void GoToBoard() => CurrentPage = BoardVm;

    [RelayCommand]
    private void GoToGarden() => CurrentPage = GardenVm;

    [RelayCommand]
    private void GoToSettings() => CurrentPage = SettingsVm;
}