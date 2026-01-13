using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public class SakuraNightTheme : ITheme
{
    public string Name => "Sakura Night";

    // A deep, dark "Midnight Plum" for the main board
    public Color BackgroundMain => Color.Parse("#1A161C"); 
    
    // An even deeper shade for the sidebar to create a shadow effect
    public Color BackgroundSidebar => Color.Parse("#141115"); 
    
    // A slightly lighter purple-grey for cards
    public Color BackgroundCard => Color.Parse("#252029"); 
    
    // The "Sakura Pink" - bright enough to pop but soft enough for dark mode
    public Color AccentColor => Color.Parse("#E8A2C8"); 
    
    // Off-white with a hint of rose for primary text
    public Color TextMain => Color.Parse("#F2E9F0"); 
    
    // Subdued dusty lavender for metadata
    public Color TextMuted => Color.Parse("#9B8B9A"); 
    
    // Subtle border that leans into the purple tones
    public Color BorderColor => Color.Parse("#352E3A");

    // Lighter purple tint for hover feedback
    public Color BackgroundCardHover => Color.Parse("#312A36");

    // Layout Specs: Sleek and rounded
    public CornerRadius StandardCornerRadius => new CornerRadius(10);
    public Thickness TaskCardBorderThickness => new Thickness(4, 0, 0, 0);
    
    public ThemeVariant BaseVariant => ThemeVariant.Dark;
}