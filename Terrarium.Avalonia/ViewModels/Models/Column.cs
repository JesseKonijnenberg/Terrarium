using System.Collections.ObjectModel;
using Terrarium.Avalonia.ViewModels.Core;
using Terrarium.Core.Models.Kanban;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class Column : ViewModelBase
    {
        private readonly ColumnEntity _entity;
        public ColumnEntity Entity => _entity;

        public Column(ColumnEntity entity)
        {
            _entity = entity;
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

        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public int TaskCount => Tasks.Count;
    }
}