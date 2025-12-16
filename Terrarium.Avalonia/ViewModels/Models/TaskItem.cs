using Avalonia.Media;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class TaskItem : ViewModelBase
    {
        private string _id = "";
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _content = "";
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _tag = "";
        public string Tag
        {
            get => _tag;
            set
            {
                if (_tag != value)
                {
                    _tag = value.ToUpper();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TagBgColor));
                    OnPropertyChanged(nameof(TagTextColor));
                    OnPropertyChanged(nameof(TagBorderColor));
                }
            }
        }

        private string _priority = "";
        public string Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHighPriority));
                }
            }
        }

        private string _date = "";
        public string Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsHighPriority => Priority == "High";

        public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
        public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
        public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);

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