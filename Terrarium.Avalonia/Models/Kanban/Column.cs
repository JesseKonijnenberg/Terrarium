using System.Collections.ObjectModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.Models.Kanban
{
    public class Column(ColumnEntity entity) : ViewModelBase
    {
        public ColumnEntity Entity { get; } = entity;

        public string Id => Entity.Id;

        public string Title
        {
            get => Entity.Title;
            set
            {
                if (Entity.Title == value) return;
                Entity.Title = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public int TaskCount => Tasks.Count;
    }
}