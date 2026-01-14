using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Helpers.Theme;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Events.Theming;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Avalonia.ViewModels;

public partial class MainWindowViewModel2 : ViewModelBase
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IThemeService _themeService;
    
    public ObservableCollection<OrganizationEntity> Organizations { get; } = new();
    public ObservableCollection<WorkspaceEntity> CurrentWorkspaces { get; } = new();
    public ObservableCollection<ProjectEntity> CurrentProjects { get; } = new();
    
    public KanbanBoardViewModel BoardVm { get; }
    public GardenViewModel GardenVm { get; }
    public SettingsViewModel SettingsVm { get; }

    [ObservableProperty] private ViewModelBase _currentPage;
    
    [ObservableProperty] private OrganizationEntity? _selectedOrganization;
    [ObservableProperty] private WorkspaceEntity? _selectedWorkspace;
    [ObservableProperty] private ProjectEntity? _selectedProject;

    public string SelectedOrgInitial => SelectedOrganization?.Name?.FirstOrDefault().ToString() ?? "P";

    public MainWindowViewModel2(
        KanbanBoardViewModel boardVm,
        GardenViewModel gardenVm,
        SettingsViewModel settingsVm,
        IHierarchyService hierarchyService,
        IThemeService themeService)
    {
        BoardVm = boardVm;
        GardenVm = gardenVm;
        SettingsVm = settingsVm;
        _hierarchyService = hierarchyService;
        _currentPage = BoardVm;
        _themeService = themeService;
        
        _themeService.ThemeChanged += OnThemeChanged;
        
        _ = LoadHierarchyAsync();
    }

    [RelayCommand]
    private async Task LoadHierarchyAsync()
    {
        var data = await _hierarchyService.GetUserHierarchyAsync();
        Organizations.Clear();
        foreach (var org in data) Organizations.Add(org);
        
        SelectedOrganization = Organizations.FirstOrDefault();
    }

    partial void OnSelectedOrganizationChanged(OrganizationEntity? value)
    {
        if (value == null) return;
        
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
        if (value != null && SelectedWorkspace != null)
        {
            BoardVm.CurrentWorkspaceId = SelectedWorkspace.Id;
            BoardVm.CurrentProjectId = value.Id;
        
            _ = BoardVm.LoadDataAsync();
        }
    }
    
    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SelectedOrgInitial));
        OnPropertyChanged(nameof(SelectedOrganization));
        
        OnPropertyChanged(nameof(Organizations));
        OnPropertyChanged(nameof(CurrentWorkspaces));
        OnPropertyChanged(nameof(CurrentProjects));
    }
    
    private void ApplyThemeForOrganization(OrganizationEntity org)
    {
        var theme = _themeService.GetThemeForOrganization(org);
        ThemeManager.ApplyTheme(theme);
    }

    [RelayCommand] private void SelectOrganization(OrganizationEntity org) => SelectedOrganization = org;
    [RelayCommand] private void SelectWorkspace(WorkspaceEntity ws) => SelectedWorkspace = ws;
    [RelayCommand] private void SelectProject(ProjectEntity proj) => SelectedProject = proj;
 
    [RelayCommand] private void GoToBoard() => CurrentPage = BoardVm;
    [RelayCommand] private void GoToGarden() => CurrentPage = GardenVm;
    [RelayCommand] private void GoToSettings() => CurrentPage = SettingsVm;
}