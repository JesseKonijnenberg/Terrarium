using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Models.Hierarchy;

namespace Terrarium.Avalonia.ViewModels;

/// <summary>
/// The main shell of the application, managing top-level navigation between views.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IHierarchyService _hierarchyService;
    
    public ObservableCollection<OrganizationEntity> Organizations { get; } = new();
    
    public KanbanBoardViewModel BoardVm { get; }
    public GardenViewModel GardenVm { get; }
    public SettingsViewModel SettingsVm { get; }

    /// <summary>
    /// The current view being displayed in the main content area.
    /// The toolkit generates the 'CurrentPage' property automatically.
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentPage;
    
    [ObservableProperty]
    private WorkspaceEntity? _selectedWorkspace;

    public MainWindowViewModel(
        KanbanBoardViewModel boardVm,
        GardenViewModel gardenVm,
        SettingsViewModel settingsVm,
        IHierarchyService hierarchyService)
    {
        BoardVm = boardVm;
        GardenVm = gardenVm;
        SettingsVm = settingsVm;
        _hierarchyService = hierarchyService;
        
        _currentPage = BoardVm; // Default starting page
        _ = LoadHierarchyAsync();
    }

    [RelayCommand]
    private void GoToBoard() => CurrentPage = BoardVm;

    [RelayCommand]
    private void GoToGarden() => CurrentPage = GardenVm;

    [RelayCommand]
    private void GoToSettings() => CurrentPage = SettingsVm;
    
    [RelayCommand]
    private void SelectWorkspace(WorkspaceEntity workspace)
    {
        SelectedWorkspace = workspace;
    }
    
    private async Task LoadHierarchyAsync()
    {
        var data = await _hierarchyService.GetUserHierarchyAsync();
        Organizations.Clear();
        foreach (var org in data) Organizations.Add(org);
        
        // Auto-select the first workspace so the board isn't empty on launch
        if (SelectedWorkspace == null)
        {
            SelectedWorkspace = Organizations.FirstOrDefault()?.Workspaces.FirstOrDefault();
        }
    }

    partial void OnSelectedWorkspaceChanged(WorkspaceEntity? value)
    {
        if (value != null)
        {
            // Update the board's scope
            BoardVm.CurrentWorkspaceId = value.Id;
            
            // Ensure we are looking at the Board page
            if (CurrentPage != BoardVm) CurrentPage = BoardVm;
        }
    }
}