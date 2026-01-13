namespace Terrarium.Core.Models.Theming;

public interface ITheme
{
    string Id { get; }
    string Name { get; }
    
    // Core uses strings to stay platform-agnostic
    string BackgroundMain { get; }
    string BackgroundSidebar { get; }
    string BackgroundCard { get; }
    string AccentColor { get; }
    string TextMain { get; }
    string TextMuted { get; }
    string BorderColor { get; }
    string BackgroundCardHover { get; }

    // Layout as doubles/simple types
    double CornerRadius { get; }
    double AccentBorderThickness { get; }
    
    // "IsDark" bool replaces Avalonia-specific ThemeVariant
    bool IsDark { get; }
}