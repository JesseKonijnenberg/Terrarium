using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public class MidnightNordTheme : ITheme
{
    public string Name => "Midnight Nord";

    // Deep, dark blue-grey for the board
    public Color BackgroundMain => Color.Parse("#242933"); 
    
    // Darker shade for the sidebar to create depth
    public Color BackgroundSidebar => Color.Parse("#1B1E25"); 
    
    // Slightly lifted blue for the cards
    public Color BackgroundCard => Color.Parse("#2E3440"); 
    
    // A frosty "Nord" blue for selection and highlights
    public Color AccentColor => Color.Parse("#88C0D0"); 
    
    // Clean, off-white for primary text
    public Color TextMain => Color.Parse("#ECEFF4"); 
    
    // Muted blue-grey for secondary info
    public Color TextMuted => Color.Parse("#D8DEE9"); 
    
    // Subtle border to define elements without being harsh
    public Color BorderColor => Color.Parse("#3B4252");

    // Lighter blue-grey for hover feedback
    public Color BackgroundCardHover => Color.Parse("#3B4252");

    // Layout Specs: Keeping it very clean and slightly more rounded
    public CornerRadius StandardCornerRadius => new CornerRadius(12);
    public Thickness TaskCardBorderThickness => new Thickness(4, 0, 0, 0);
    
    public ThemeVariant BaseVariant => ThemeVariant.Dark;
}