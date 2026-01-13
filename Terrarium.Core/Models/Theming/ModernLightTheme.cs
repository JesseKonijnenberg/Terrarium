namespace Terrarium.Core.Models.Theming;

public class ModernLightTheme : ITheme
{
    public string Id => "modern-light";
    public string Name => "Modern Light";
    public string BackgroundMain => "#F9F8F6";
    public string BackgroundSidebar => "#F0EEE9";
    public string BackgroundCard => "#FFFFFF";
    public string AccentColor => "#8FA172";
    public string TextMain => "#2D2B29";
    public string TextMuted => "#7A7774";
    public string BorderColor => "#E0DED9";
    public string BackgroundCardHover => "#F2F1EE";

    public double CornerRadius => 10.0;
    public double AccentBorderThickness => 5.0;
    public bool IsDark => false;
}