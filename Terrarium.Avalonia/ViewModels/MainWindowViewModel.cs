using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Context;

namespace Terrarium.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IProjectContextService _projectContextService;

    public INavigationService Nav { get; }

    public MainWindowViewModel(INavigationService navigationService, IProjectContextService projectContextService)
    {
        Nav = navigationService;
        _projectContextService = projectContextService;

        // 1. Set the initial Shell (Landing screen)
        // Nav.SetRoot<LandingViewModel>(); 
        
        Nav.SetRoot<WorkspaceViewModel>();
        Nav.NavigateContent<KanbanBoardViewModel>(clearHistory: true);
    }
}