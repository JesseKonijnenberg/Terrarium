namespace Terrarium.Core.Models.Theming;

public class MidnightNordTheme : ITheme
{
    public string Id => "midnight-nord";
    public string Name => "Midnight Nord";
    public string BackgroundMain => "#242933";
    public string BackgroundSidebar => "#1B1E25";
    public string BackgroundCard => "#2E3440";
    public string AccentColor => "#88C0D0";
    public string TextMain => "#ECEFF4";
    public string TextMuted => "#D8DEE9";
    public string BorderColor => "#3B4252";
    public string BackgroundCardHover => "#3B4252";

    public double CornerRadius => 12.0;
    public double AccentBorderThickness => 4.0;
    public bool IsDark => true;
}