using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.Helpers.Theme;
using Terrarium.Avalonia.Models.Theme;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public string AppVersion => $"v{Helpers.AppVersion.Get()}";
    public List<ITheme> AvailableThemes { get; } = new()
    {
        new SageDarkTheme(),
        new ModernLightTheme(),
        new MidnightNordTheme(),
        new TerrariumLinenTheme()
        // To add a new theme, just add 'new MyNewTheme(),' here!
    };

    [ObservableProperty]
    private bool _enableNotifications = true;
    
    [ObservableProperty]
    private ITheme _selectedTheme;

    [ObservableProperty]
    private bool _autoSaveEnabled = true;

    public SettingsViewModel()
    {
        _selectedTheme = AvailableThemes[0];
        ThemeManager.ApplyTheme(_selectedTheme);
    }

    /// <summary>
    /// Community Toolkit hook: Triggered automatically when the Theme changes.
    /// This is where you would call your actual ThemeManager logic.
    /// </summary>
    partial void OnSelectedThemeChanged(ITheme? value)
    {
        if (value == null) return;
        ThemeManager.ApplyTheme(value);
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        EnableNotifications = true;
        AutoSaveEnabled = true;
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        //TODO implement exportdata
    }
}