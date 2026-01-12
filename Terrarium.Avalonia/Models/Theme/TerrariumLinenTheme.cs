using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public class TerrariumLinenTheme : ITheme
{
    public string Name => "Terrarium Linen";

    // A warm, soft grey-white that feels like high-quality paper
    public Color BackgroundMain => Color.Parse("#F5F5F3"); 
    
    // Slightly more "compressed" tone for the sidebar to provide structure
    public Color BackgroundSidebar => Color.Parse("#EBEAE6"); 
    
    // Pure white for cards makes them feel elevated and clean
    public Color BackgroundCard => Color.Parse("#FFFFFF"); 
    
    // A deep, sophisticated forest green (Darker than Sage for readability)
    public Color AccentColor => Color.Parse("#4A6741"); 
    
    // Deep charcoal, much softer on the eyes than pure black
    public Color TextMain => Color.Parse("#0f0f0e"); 
    
    // A warm grey for dates and secondary info
    public Color TextMuted => Color.Parse("#7D7A74"); 
    
    // Very subtle border that disappears into the background
    public Color BorderColor => Color.Parse("#DEDCD4");

    // A very subtle "shadow" tint for hover states
    public Color BackgroundCardHover => Color.Parse("#FBFBFA");

    // Layout Specs: Rounder corners for a friendly, modern feel
    public CornerRadius StandardCornerRadius => new CornerRadius(14);
    public Thickness TaskCardBorderThickness => new Thickness(6, 0, 0, 0);
    
    public ThemeVariant BaseVariant => ThemeVariant.Light;
}