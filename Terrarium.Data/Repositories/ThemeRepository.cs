using System.Diagnostics;
using System.Text.Json;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Models.Theming;

namespace Terrarium.Data.Repositories;

public class ThemeRepository : IThemeRepository
{
    private readonly List<ITheme> _themes = new();
    private readonly string _externalThemesPath;

    public ThemeRepository(string? externalThemesPath = null)
    {
        // Default to AppData if no path is provided
        _externalThemesPath = externalThemesPath ?? 
                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Terrarium", "Themes");

        ReloadThemes();
    }

    public void ReloadThemes()
    {
        _themes.Clear();

        // 1. Add hardcoded built-in themes
        _themes.Add(new SageDarkTheme());
        _themes.Add(new ModernLightTheme());
        _themes.Add(new MidnightNordTheme());
        _themes.Add(new TerrariumLinenTheme());
        _themes.Add(new SakuraNightTheme());

        // 2. Load custom themes from JSON files
        LoadExternalThemes();
    }

    private void LoadExternalThemes()
    {
        if (!Directory.Exists(_externalThemesPath))
        {
            Directory.CreateDirectory(_externalThemesPath);
            return;
        }

        var files = Directory.GetFiles(_externalThemesPath, "*.json");
        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                // JsonTheme is a POCO in Core that implements ITheme
                var externalTheme = JsonSerializer.Deserialize<JsonTheme>(json);
                if (externalTheme != null) _themes.Add(externalTheme);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load theme {file}: {ex.Message}");
            }
        }
    }

    public IEnumerable<ITheme> GetAllThemes() => _themes;

    public ITheme? GetThemeById(string id) => _themes.FirstOrDefault(t => t.Id == id);
}