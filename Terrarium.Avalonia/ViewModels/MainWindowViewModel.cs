using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Context;

namespace Terrarium.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IProjectContextService _projectContextService;

    // These properties drive the TransitioningContentControls in XAML
    [ObservableProperty] private ViewModelBase? _rootState;
    [ObservableProperty] private ViewModelBase? _currentContent;

    public MainWindowViewModel(INavigationService navigationService,IProjectContextService projectContextService)
    {
        _navigationService = navigationService;
        _projectContextService = projectContextService;

        // Subscribe to navigation changes
        _navigationService.NavigationChanged += OnNavigationChanged;

        // 1. Set the initial Shell (Landing screen)
        // _navigationService.SetRoot<LandingViewModel>(); 
        
        _navigationService.SetRoot<WorkspaceViewModel>();
        _navigationService.NavigateContent<KanbanBoardViewModel>(clearHistory: true);
    }

    private void OnNavigationChanged()
    {
        RootState = _navigationService.RootState;
        CurrentContent = _navigationService.CurrentContent;
    }
}