using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Helpers.Theme;
using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Avalonia.ViewModels;

public partial class SidebarViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IHierarchyService _hierarchyService;
    private readonly IThemeService _themeService;
    private readonly KanbanBoardViewModel _boardVm;
    private readonly IProjectContextService _contextService;

    // Collections moved from MainWindow
    public ObservableCollection<OrganizationEntity> Organizations { get; } = new();
    public ObservableCollection<WorkspaceEntity> CurrentWorkspaces { get; } = new();
    public ObservableCollection<ProjectEntity> CurrentProjects { get; } = new();

    [ObservableProperty] private OrganizationEntity? _selectedOrganization;
    [ObservableProperty] private WorkspaceEntity? _selectedWorkspace;
    [ObservableProperty] private ProjectEntity? _selectedProject;

    public string SelectedOrgInitial => SelectedOrganization?.Name?.FirstOrDefault().ToString() ?? "P";
    
    public UpdateViewModel Updater { get; }

    public SidebarViewModel(
        INavigationService navigationService,
        IHierarchyService hierarchyService,
        IThemeService themeService,
        UpdateViewModel updater,
        KanbanBoardViewModel boardVm,
        IProjectContextService contextService)
    
    {
        _navigationService = navigationService;
        _hierarchyService = hierarchyService;
        _themeService = themeService;
        _boardVm = boardVm;
        _contextService = contextService;
        Updater = updater;

        // Initialize hierarchy
        _ = LoadHierarchyAsync();
    }

    [RelayCommand]
    public async Task LoadHierarchyAsync()
    {
        var data = await _hierarchyService.GetUserHierarchyAsync();
        Organizations.Clear();
        foreach (var org in data) Organizations.Add(org);
    
        var initialOrg = Organizations.FirstOrDefault();
        if (initialOrg != null)
        {
            SelectedOrganization = initialOrg;
            
            var firstWs = initialOrg.Workspaces.FirstOrDefault();
            var firstProj = firstWs?.Projects.FirstOrDefault();

            if (firstWs != null && firstProj != null)
            {
                _contextService.UpdateContext(initialOrg.Id, firstWs.Id, firstProj.Id);
            }
        }
    }

    // Logic moved from MainWindow partial methods
    partial void OnSelectedOrganizationChanged(OrganizationEntity? value)
    {
        if (value == null) return;
        
        // Apply theme for the new organization
        var theme = _themeService.GetThemeForOrganization(value);
        ThemeManager.ApplyTheme(theme);
        
        CurrentWorkspaces.Clear();
        if (value.Workspaces != null)
        {
            foreach (var ws in value.Workspaces) CurrentWorkspaces.Add(ws);
        }
    
        SelectedWorkspace = CurrentWorkspaces.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedOrgInitial));
    }

    partial void OnSelectedWorkspaceChanged(WorkspaceEntity? value)
    {
        CurrentProjects.Clear();
        if (value?.Projects != null)
        {
            foreach (var proj in value.Projects) CurrentProjects.Add(proj);
        }
        
        SelectedProject = CurrentProjects.FirstOrDefault();
    }

    partial void OnSelectedProjectChanged(ProjectEntity? value)
    {
        if (value != null && SelectedWorkspace != null && SelectedOrganization != null)
        {
            // Update the central service
            _contextService.UpdateContext(
                SelectedOrganization.Id, 
                SelectedWorkspace.Id, 
                value.Id);
        }
    }

    // Navigation Commands using the new Service
    [RelayCommand] private void GoToBoard() => _navigationService.NavigateContent<KanbanBoardViewModel>();
    [RelayCommand] private void GoToGarden() => _navigationService.NavigateContent<GardenViewModel>();
    [RelayCommand] private void GoToSettings() => _navigationService.NavigateContent<SettingsViewModel>();

    // Selection Commands (used by UI ListBoxes/Buttons)
    [RelayCommand] private void SelectOrganization(OrganizationEntity org) => SelectedOrganization = org;
    [RelayCommand] private void SelectWorkspace(WorkspaceEntity ws) => SelectedWorkspace = ws;
    [RelayCommand] private void SelectProject(ProjectEntity proj) => SelectedProject = proj;
}