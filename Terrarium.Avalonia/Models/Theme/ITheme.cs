using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public interface ITheme
{
    string Name { get; }
    Color BackgroundMain { get; }
    Color BackgroundSidebar { get; }
    Color BackgroundCard { get; }
    Color AccentColor { get; }
    Color TextMain { get; }
    Color TextMuted { get; }
    Color BorderColor { get; }
    Color BackgroundCardHover { get; }
    
    // Layout and Shape
    CornerRadius StandardCornerRadius { get; }
    Thickness TaskCardBorderThickness { get; }
    
    // The underlying Avalonia variant (Light or Dark)
    ThemeVariant BaseVariant { get; }
}