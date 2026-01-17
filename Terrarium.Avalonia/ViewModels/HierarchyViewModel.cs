using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Helpers.Theme;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Avalonia.ViewModels;

public partial class HierarchyViewModel : ViewModelBase
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IOrganizationService _orgService;
    private readonly IWorkspaceService _wsService;
    private readonly IProjectService _projectService;
    private readonly IProjectContextService _contextService;
    private readonly IThemeService _themeService;
    private readonly IDialogService _dialogService;

    public ObservableCollection<OrganizationEntity> Organizations { get; } = new();
    public ObservableCollection<WorkspaceEntity> Workspaces { get; } = new();
    public ObservableCollection<ProjectEntity> Projects { get; } = new();

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(SelectedOrgInitial))]
    private OrganizationEntity? _selectedOrganization;

    [ObservableProperty] private WorkspaceEntity? _selectedWorkspace;
    [ObservableProperty] private ProjectEntity? _selectedProject;

    public string SelectedOrgInitial => SelectedOrganization?.Name?.FirstOrDefault().ToString() ?? "+";

    public HierarchyViewModel(
        IHierarchyService hierarchyService,
        IOrganizationService orgService,
        IWorkspaceService wsService,
        IProjectService projectService,
        IProjectContextService contextService,
        IThemeService themeService,
        IDialogService dialogService)
    {
        _hierarchyService = hierarchyService;
        _orgService = orgService;
        _wsService = wsService;
        _projectService = projectService;
        _contextService = contextService;
        _themeService = themeService;
        _dialogService = dialogService;

        _ = LoadHierarchyAsync();
    }

    public async Task LoadHierarchyAsync()
    {
        var data = await _hierarchyService.GetUserHierarchyAsync();
        
        Organizations.Clear();
        foreach (var org in data) Organizations.Add(org);
        
        var (dOrg, dWs, dProj) = _hierarchyService.GetDefaultSelection(data);
        SelectedOrganization = dOrg;
        SelectedWorkspace = dWs;
        SelectedProject = dProj;
    }

    partial void OnSelectedOrganizationChanged(OrganizationEntity? value)
    {
        if (value == null) return;
        
        var theme = _themeService.GetThemeForOrganization(value);
        ThemeManager.ApplyTheme(theme);
        
        Workspaces.Clear();
        if (value.Workspaces != null)
            foreach (var ws in value.Workspaces) Workspaces.Add(ws);

        SelectedWorkspace = Workspaces.FirstOrDefault();
    }

    partial void OnSelectedWorkspaceChanged(WorkspaceEntity? value)
    {
        Projects.Clear();
        if (value?.Projects != null)
            foreach (var p in value.Projects) Projects.Add(p);

        SelectedProject = Projects.FirstOrDefault();
    }

    partial void OnSelectedProjectChanged(ProjectEntity? value)
    {
        if (value != null && SelectedOrganization != null && SelectedWorkspace != null)
        {
            _contextService.UpdateContext(SelectedOrganization.Id, SelectedWorkspace.Id, value.Id);
        }
    }

    [RelayCommand]
    private async Task CreateOrganizationAsync()
    {
        var name = await _dialogService.ShowInputAsync("New Organization", "Enter Organization Name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var newOrg = await _orgService.CreateOrganizationAsync(name);
        Organizations.Add(newOrg);
        SelectedOrganization = newOrg;
    }

    [RelayCommand]
    private async Task CreateWorkspaceAsync()
    {
        if (SelectedOrganization == null) return;
        
        var name = await _dialogService.ShowInputAsync("New Workspace", "Enter Workspace Name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var newWs = await _wsService.CreateWorkspaceAsync(SelectedOrganization.Id, name);
        
        SelectedOrganization.Workspaces ??= new List<WorkspaceEntity>();
        SelectedOrganization.Workspaces.Add(newWs);
        
        Workspaces.Add(newWs);
        SelectedWorkspace = newWs;
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        if (SelectedWorkspace == null) return;

        var name = await _dialogService.ShowInputAsync("New Project", "Enter Project Name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var newProj = await _projectService.CreateProjectAsync(SelectedWorkspace.Id, name);

        SelectedWorkspace.Projects ??= new List<ProjectEntity>();
        SelectedWorkspace.Projects.Add(newProj);

        Projects.Add(newProj);
        SelectedProject = newProj;
    }

    [RelayCommand]
    private async Task DeleteProjectAsync(ProjectEntity project)
    {
        if (!await _dialogService.ConfirmAsync("Delete Project", $"Delete '{project.Name}' and all its tasks?")) return;

        await _projectService.DeleteProjectAsync(project.Id);
        
        Projects.Remove(project);
        SelectedWorkspace?.Projects?.Remove(project);

        if (SelectedProject == project) 
            SelectedProject = Projects.FirstOrDefault();
    }
}