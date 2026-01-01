using System;
using Avalonia.Media;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.Models.Kanban;

public class TaskItem(TaskEntity entity) : ViewModelBase
{
    public TaskEntity Entity { get; } = entity ?? throw new ArgumentNullException(nameof(entity));

    public string Id => Entity.Id;

    public string Title
    {
        get => Entity.Title;
        set { if (Entity.Title == value) return; Entity.Title = value; OnPropertyChanged(); }
    }

    public string Description
    {
        get => Entity.Description;
        set { if (Entity.Description == value) return; Entity.Description = value; OnPropertyChanged(); }
    }

    public string Tag
    {
        get => Entity.Tag;
        set
        {
            if (Entity.Tag == value) return;
            Entity.Tag = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TagBgColor));
            OnPropertyChanged(nameof(TagTextColor));
            OnPropertyChanged(nameof(TagBorderColor));
        }
    }

    public string Priority
    {
        get => Entity.Priority.ToString();
        set
        {
            if (!Enum.TryParse<TaskPriority>(value, true, out var newPriority)) return;
            if (Entity.Priority == newPriority) return;
            
            Entity.Priority = newPriority;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHighPriority));
        }
    }

    public string Date
    {
        get => Entity.DueDate.ToString("MMM dd");
        set
        {
            if (!DateTime.TryParse(value, out var newDate)) return;
            Entity.DueDate = newDate;
            OnPropertyChanged();
        }
    }

    public bool IsHighPriority => Entity.Priority == TaskPriority.High;

    // UI Formatting Logic
    public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
    public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
    public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);

    private IBrush GetTagBrush(string? tag, double opacity)
    {
        var colorStr = tag?.ToUpper() switch
        {
            "DESIGN" => "#a65d57",
            "DEV" => "#4a5c6a",
            "MARKETING" => "#cca43b",
            "PRODUCT" => "#5e6c5b",
            _ => "#5e6c5b"
        };

        var baseColor = Color.Parse(colorStr);
        return new SolidColorBrush(new Color((byte)(255 * opacity), baseColor.R, baseColor.G, baseColor.B));
    }
}