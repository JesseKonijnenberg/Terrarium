using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.Helpers.Theme;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Theming;

namespace Terrarium.Avalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly IHierarchyService _hierarchyService;

    public string AppVersion => $"v{Helpers.AppVersion.Get()}";

    // Themes are now resolved through the Service
    public List<ITheme> AvailableThemes { get; }

    [ObservableProperty]
    private ITheme _selectedTheme;

    // We check the ActiveOrganization from the HierarchyService
    public bool CanChangeTheme => _hierarchyService.ActiveOrganization?.LockTheme == false;

    public SettingsViewModel(IThemeService themeService, IHierarchyService hierarchyService)
    {
        _themeService = themeService;
        _hierarchyService = hierarchyService;

        // 1. Populate the list from the service
        AvailableThemes = _themeService.GetAvailableThemes().ToList();
        
        // 2. Resolve the theme for the current active organization
        var currentOrg = _hierarchyService.ActiveOrganization;
        if (currentOrg != null)
        {
            _selectedTheme = _themeService.GetThemeForOrganization(currentOrg);
        }
        else
        {
            // Fallback if no org is selected yet
            _selectedTheme = AvailableThemes.FirstOrDefault()!;
        }
    }

    partial void OnSelectedThemeChanged(ITheme? value)
    {
        if (value == null) return;

        var currentOrg = _hierarchyService.ActiveOrganization;
        if (currentOrg != null && !currentOrg.LockTheme)
        {
            _themeService.TrySetOrganizationTheme(currentOrg, value);
            ThemeManager.ApplyTheme(value);
        }
    }
}