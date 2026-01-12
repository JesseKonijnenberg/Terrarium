using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public class ModernLightTheme : ITheme
{
    public string Name => "Modern Light";
    
    // Soft off-white for the main background
    public Color BackgroundMain => Color.Parse("#F9F8F6"); 
    
    // Slightly darker grey-beige for sidebars
    public Color BackgroundSidebar => Color.Parse("#F0EEE9"); 
    
    // Clean white for cards to make them "pop"
    public Color BackgroundCard => Color.Parse("#FFFFFF"); 
    
    // Your signature Sage, slightly darkened for better contrast on light
    public Color AccentColor => Color.Parse("#8FA172"); 
    
    // High-contrast charcoal for readability
    public Color TextMain => Color.Parse("#2D2B29"); 
    
    // Subdued grey for metadata
    public Color TextMuted => Color.Parse("#7A7774"); 
    
    // Subtle border color
    public Color BorderColor => Color.Parse("#E0DED9");
    
    public Color BackgroundCardHover => Color.Parse("#F2F1EE");

    // Layout Specs
    public CornerRadius StandardCornerRadius => new CornerRadius(10);
    public Thickness TaskCardBorderThickness => new Thickness(5, 0, 0, 0);
    
    // CRITICAL: Tells Avalonia to use light-mode logic for internal controls
    public ThemeVariant BaseVariant => ThemeVariant.Light; 
}