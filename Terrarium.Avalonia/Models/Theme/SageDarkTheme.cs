using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace Terrarium.Avalonia.Models.Theme;

public class SageDarkTheme : ITheme
{
    public string Name => "Sage Dark";
    public Color BackgroundMain => Color.Parse("#1c1a19");
    public Color BackgroundSidebar => Color.Parse("#252321");
    public Color BackgroundCard => Color.Parse("#2c2a28");
    public Color AccentColor => Color.Parse("#b5c18e");
    public Color TextMain => Color.Parse("#ffffff");
    public Color TextMuted => Color.Parse("#898989");
    public Color BorderColor => Color.Parse("#3d3a38");
    public Color BackgroundCardHover => Color.Parse("#353331");

    public CornerRadius StandardCornerRadius => new CornerRadius(8);
    public Thickness TaskCardBorderThickness => new Thickness(4, 0, 0, 0);
    public ThemeVariant BaseVariant => ThemeVariant.Dark;
}