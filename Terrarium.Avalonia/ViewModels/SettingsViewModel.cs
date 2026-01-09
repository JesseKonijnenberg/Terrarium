using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public string AppVersion => $"v{Helpers.AppVersion.Get()}";

    [ObservableProperty]
    private bool _enableNotifications = true;

    [ObservableProperty]
    private string _theme = "Dark";

    [ObservableProperty]
    private bool _autoSaveEnabled = true;

    public SettingsViewModel()
    {
    }

    /// <summary>
    /// Community Toolkit hook: Triggered automatically when the Theme changes.
    /// This is where you would call your actual ThemeManager logic.
    /// </summary>
    partial void OnThemeChanged(string value)
    {
        //TODO implement themechange
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        EnableNotifications = true;
        Theme = "Dark";
        AutoSaveEnabled = true;
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        //TODO implement exportdata
    }
}