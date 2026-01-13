namespace Terrarium.Core.Models.Theming;

public class TerrariumLinenTheme : ITheme
{
    public string Id => "terrarium-linen";
    public string Name => "Terrarium Linen";
    public string BackgroundMain => "#F5F5F3";
    public string BackgroundSidebar => "#EBEAE6";
    public string BackgroundCard => "#FFFFFF";
    public string AccentColor => "#4A6741";
    public string TextMain => "#0f0f0e";
    public string TextMuted => "#7D7A74";
    public string BorderColor => "#DEDCD4";
    public string BackgroundCardHover => "#FBFBFA";

    public double CornerRadius => 14.0;
    public double AccentBorderThickness => 6.0;
    public bool IsDark => false;
}