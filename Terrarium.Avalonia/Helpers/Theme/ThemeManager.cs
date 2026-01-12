using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Terrarium.Avalonia.Models.Theme;

namespace Terrarium.Avalonia.Helpers.Theme;

public static class ThemeManager
{
    public static void ApplyTheme(ITheme theme)
    {
        // Important: Get the root resource dictionary
        var res = Application.Current!.Resources;

        // 1. Base Variant (Switches internal Fluent control styles)
        Application.Current.RequestedThemeVariant = theme.BaseVariant;

        // 2. Explicitly overwrite the keys
        // We use indexer access to ensure we overwrite existing keys
        res["BgMain"] = new SolidColorBrush(theme.BackgroundMain);
        res["BgSidebar"] = new SolidColorBrush(theme.BackgroundSidebar);
        res["BgCard"] = new SolidColorBrush(theme.BackgroundCard);
        res["AccentSage"] = new SolidColorBrush(theme.AccentColor);
        res["TextMain"] = new SolidColorBrush(theme.TextMain);
        res["TextMuted"] = new SolidColorBrush(theme.TextMuted);
        res["BorderColor"] = new SolidColorBrush(theme.BorderColor);
        res["BgCardHover"] = new SolidColorBrush(theme.BackgroundCardHover);

        res["MainCornerRadius"] = theme.StandardCornerRadius;
        res["TaskCardBorderThickness"] = theme.TaskCardBorderThickness;

        // 3. Update Fluent Theme Palette
        if (Application.Current.Styles.Count > 0 && Application.Current.Styles[0] is FluentTheme fluent)
        {
            UpdateFluentPalette(fluent, theme);
        }
    }

    private static void UpdateFluentPalette(FluentTheme fluent, ITheme theme)
    {
        // We create a new palette resources object based on the theme's requirements
        var palette = new ColorPaletteResources
        {
            Accent = theme.AccentColor,
            // You can map more palette colors here if needed (RegionColor, AltHigh, etc.)
        };

        // Clear existing and set the specific palette for the current mode
        if (theme.BaseVariant == ThemeVariant.Dark)
        {
            fluent.Palettes.Remove(ThemeVariant.Dark);
            fluent.Palettes.Add(ThemeVariant.Dark, palette);
        }
        else
        {
            fluent.Palettes.Remove(ThemeVariant.Light);
            fluent.Palettes.Add(ThemeVariant.Light, palette);
        }
    }
}