namespace Terrarium.Core.Models.Theming;

public class SakuraNightTheme : ITheme
{
    public string Id => "sakura-night";
    public string Name => "Sakura Night";
    
    public string BackgroundMain => "#1A161C";
    public string BackgroundSidebar => "#141115";
    public string BackgroundCard => "#252029";
    public string AccentColor => "#E8A2C8";
    public string TextMain => "#F2E9F0";
    public string TextMuted => "#9B8B9A";
    public string BorderColor => "#352E3A";
    public string BackgroundCardHover => "#312A36";

    public double CornerRadius => 10.0;
    public double AccentBorderThickness => 4.0;
    public bool IsDark => true;
}