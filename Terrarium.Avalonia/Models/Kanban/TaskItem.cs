using System;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.Models.Kanban;

/// <summary>
/// A reactive wrapper for TaskEntity that provides UI-ready properties 
/// and automatic brush calculation.
/// </summary>
public partial class TaskItem : ViewModelBase
{
    public TaskEntity Entity { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TagBgColor))]
    [NotifyPropertyChangedFor(nameof(TagTextColor))]
    [NotifyPropertyChangedFor(nameof(TagBorderColor))]
    private string _tag;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHighPriority))]
    private TaskPriority _priority;

    [ObservableProperty]
    private DateTime _dueDate;

    [ObservableProperty]
    private bool _isSelected;
    
    public string? ProjectId => Entity.ProjectId;
    public string? IterationId => Entity.IterationId;
    public string ColumnId => Entity.ColumnId;

    public TaskItem(TaskEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        
        _title = entity.Title;
        _description = entity.Description;
        _tag = entity.Tag;
        _priority = entity.Priority;
        _dueDate = entity.DueDate;
    }

    public string Id => Entity.Id;
    public bool IsHighPriority => Priority == TaskPriority.High;
    
    public string FormattedDate => DueDate.ToString("MMM dd");

    public IBrush TagBgColor => GetTagBrush(Tag, 0.3);
    public IBrush TagTextColor => GetTagBrush(Tag, 1.0);
    public IBrush TagBorderColor => GetTagBrush(Tag, 0.5);

    // Callbacks to sync UI changes back to the Entity (One-Way to Source)
    partial void OnTitleChanged(string value) => Entity.Title = value;
    partial void OnDescriptionChanged(string value) => Entity.Description = value;
    partial void OnTagChanged(string value) => Entity.Tag = value;
    partial void OnPriorityChanged(TaskPriority value) => Entity.Priority = value;
    partial void OnDueDateChanged(DateTime value) => Entity.DueDate = value;

    /// <summary>
    /// Force updates the UI properties to match the current state of the underlying Entity.
    /// Call this after bulk-updating the Entity from the database.
    /// </summary>
    public void RefreshFromEntity()
    {
        // We set the properties, which triggers OnPropertyChanged.
        // (The setters will write back to Entity, which is redundant but harmless here)
        if (Title != Entity.Title) Title = Entity.Title;
        if (Description != Entity.Description) Description = Entity.Description;
        if (Tag != Entity.Tag) Tag = Entity.Tag;
        if (Priority != Entity.Priority) Priority = Entity.Priority;
        if (DueDate != Entity.DueDate) DueDate = Entity.DueDate;
    }

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
    
    public static TaskPriority[] AllPriorities => (TaskPriority[])Enum.GetValues(typeof(TaskPriority));
}