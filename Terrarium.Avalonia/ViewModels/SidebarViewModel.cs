using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public partial class SidebarViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    
    public HierarchyViewModel Hierarchy { get; }
    public UpdateViewModel Updater { get; }

    public SidebarViewModel(
        INavigationService navigationService,
        HierarchyViewModel hierarchyViewModel,
        UpdateViewModel updater)
    {
        _navigationService = navigationService;
        Hierarchy = hierarchyViewModel;
        Updater = updater;
        
    }

    [RelayCommand] private void GoToBoard() => _navigationService.NavigateContent<KanbanBoardViewModel>();
    [RelayCommand] private void GoToGarden() => _navigationService.NavigateContent<GardenViewModel>();
    [RelayCommand] private void GoToSettings() => _navigationService.NavigateContent<SettingsViewModel>();
}