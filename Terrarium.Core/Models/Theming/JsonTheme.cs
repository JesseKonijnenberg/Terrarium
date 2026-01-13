namespace Terrarium.Core.Models.Theming;

public class JsonTheme : ITheme
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BackgroundMain { get; set; } = "#FFFFFF";
    public string BackgroundSidebar { get; set; } = "#F0F0F0";
    public string BackgroundCard { get; set; } = "#FFFFFF";
    public string AccentColor { get; set; } = "#000000";
    public string TextMain { get; set; } = "#000000";
    public string TextMuted { get; set; } = "#808080";
    public string BorderColor { get; set; } = "#E0E0E0";
    public string BackgroundCardHover { get; set; } = "#F5F5F5";

    public double CornerRadius { get; set; } = 8.0;
    public double AccentBorderThickness { get; set; } = 4.0;
    public bool IsDark { get; set; } = false;
}