using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;

    // These properties drive the TransitioningContentControls in XAML
    [ObservableProperty] private ViewModelBase? _rootState;
    [ObservableProperty] private ViewModelBase? _currentContent;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        // Subscribe to navigation changes
        _navigationService.NavigationChanged += OnNavigationChanged;

        // 1. Set the initial Shell (Landing screen)
        // _navigationService.SetRoot<LandingViewModel>(); 
        
        // FOR NOW: Let's set it to Workspace so you can see your board immediately
        _navigationService.SetRoot<WorkspaceViewModel>();
        _navigationService.NavigateContent<KanbanBoardViewModel>(clearHistory: true);
    }

    private void OnNavigationChanged()
    {
        RootState = _navigationService.RootState;
        CurrentContent = _navigationService.CurrentContent;
    }
}