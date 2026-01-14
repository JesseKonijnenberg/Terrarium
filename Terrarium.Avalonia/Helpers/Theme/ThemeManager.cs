using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Terrarium.Core.Models.Theming;

namespace Terrarium.Avalonia.Helpers.Theme;

public static class ThemeManager
{
    public static void ApplyTheme(ITheme theme)
    {
        if (Application.Current == null) return;
        
        var res = Application.Current.Resources;

        // Map Core 'IsDark' to Avalonia 'ThemeVariant'
        Application.Current.RequestedThemeVariant = theme.IsDark 
            ? ThemeVariant.Dark 
            : ThemeVariant.Light;
        
        // Using indexer access to overwrite existing keys for live updates
        res["BgMain"] = CreateBrush(theme.BackgroundMain);
        res["BgSidebar"] = CreateBrush(theme.BackgroundSidebar);
        res["BgCard"] = CreateBrush(theme.BackgroundCard);
        res["AccentSage"] = CreateBrush(theme.AccentColor);
        res["TextMain"] = CreateBrush(theme.TextMain);
        res["TextMuted"] = CreateBrush(theme.TextMuted);
        res["BorderColor"] = CreateBrush(theme.BorderColor);
        res["BgCardHover"] = CreateBrush(theme.BackgroundCardHover);
        
        res["MainCornerRadius"] = new CornerRadius(theme.CornerRadius);
        res["TaskCardBorderThickness"] = new Thickness(theme.AccentBorderThickness, 0, 0, 0);

        // Update the internal Fluent Theme palette for system controls
        if (Application.Current.Styles.Count > 0 && Application.Current.Styles[0] is FluentTheme fluent)
        {
            UpdateFluentPalette(fluent, theme);
        }
    }

    private static ISolidColorBrush CreateBrush(string hexColor)
    {
        return new SolidColorBrush(Color.Parse(hexColor));
    }

    private static void UpdateFluentPalette(FluentTheme fluent, ITheme theme)
    {
        var accentColor = Color.Parse(theme.AccentColor);
        
        var palette = new ColorPaletteResources
        {
            Accent = accentColor,
        };

        var variant = theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

        // Clear existing and set the specific palette for the current mode
        fluent.Palettes.Remove(variant);
        fluent.Palettes.Add(variant, palette);
    }
}