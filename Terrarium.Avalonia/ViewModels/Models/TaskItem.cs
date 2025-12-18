using System;
using Avalonia.Media;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class TaskItem : ViewModelBase
    {
        private readonly TaskEntity _entity;
        public TaskEntity Entity => _entity;

        public TaskItem(TaskEntity entity)
        {
            _entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public string Id => _entity.Id;

        public string Title
        {
            get => _entity.Title;
            set
            {
                if (_entity.Title != value)
                {
                    _entity.Title = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get => Entity.Description;
            set
            {
                if (Entity.Description != value)
                {
                    Entity.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Tag
        {
            get => _entity.Tag;
            set
            {
                if (_entity.Tag != value)
                {
                    _entity.Tag = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TagBgColor));
                    OnPropertyChanged(nameof(TagTextColor));
                    OnPropertyChanged(nameof(TagBorderColor));
                }
            }
        }

        public string Priority
        {
            get => _entity.Priority.ToString();
            set
            {
                if (Enum.TryParse(value, true, out TaskPriority newPriority))
                {
                    if (_entity.Priority != newPriority)
                    {
                        _entity.Priority = newPriority;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(IsHighPriority));
                    }
                }
            }
        }
        public string Date
        {
            get => _entity.DueDate.ToString("MMM dd");
            set
            {
                if (DateTime.TryParse(value, out DateTime newDate))
                {
                    _entity.DueDate = newDate;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsHighPriority => _entity.Priority == TaskPriority.High;

        public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
        public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
        public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);

        private IBrush GetTagBrush(string tag, double opacity)
        {
            var colorStr = tag?.ToUpper() switch
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