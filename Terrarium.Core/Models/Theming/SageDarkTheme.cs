namespace Terrarium.Core.Models.Theming;

public class SageDarkTheme : ITheme
{
    public string Id => "sage-dark";
    public string Name => "Sage Dark";
    public string BackgroundMain => "#1c1a19";
    public string BackgroundSidebar => "#252321";
    public string BackgroundCard => "#2c2a28";
    public string AccentColor => "#b5c18e";
    public string TextMain => "#ffffff";
    public string TextMuted => "#898989";
    public string BorderColor => "#3d3a38";
    public string BackgroundCardHover => "#353331";

    public double CornerRadius => 8.0;
    public double AccentBorderThickness => 4.0;
    public bool IsDark => true;
}