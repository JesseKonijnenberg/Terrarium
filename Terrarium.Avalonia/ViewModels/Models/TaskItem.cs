using Avalonia.Media;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class TaskItem : ViewModelBase
    {
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";

        private string _tag = "";
        public string Tag
        {
            get => _tag;
            set => _tag = value.ToUpper();
        }

        public string Priority { get; set; } = "";
        public string Date { get; set; } = "";

        public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
        public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
        public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);
        public bool IsHighPriority => Priority == "High";

        private IBrush GetTagBrush(string tag, double opacity)
        {
            var colorStr = tag.ToUpper() switch
            {
                "DESIGN" => "#a65d57",
                "DEV" => "#4a5c6a",
                "MARKETING" => "#cca43b",
                "PRODUCT" => "#5e6c5b",
                _ => "#5e6c5b"
            };

            var color = Color.Parse(colorStr);
            var finalColor = new Color((byte)(255 * opacity), color.R, color.G, color.B);
            return new SolidColorBrush(finalColor);
        }
    }
}