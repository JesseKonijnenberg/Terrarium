using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace Terrarium.Avalonia.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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
            // Logic matches the UPPERCASE tags now
            var colorStr = tag.ToUpper() switch
            {
                "DESIGN" => "#a65d57",    // Rust
                "DEV" => "#4a5c6a",       // Slate
                "MARKETING" => "#cca43b", // Gold
                "PRODUCT" => "#5e6c5b",   // Sage
                _ => "#5e6c5b"
            };

            var color = Color.Parse(colorStr);
            var finalColor = new Color((byte)(255 * opacity), color.R, color.G, color.B);
            return new SolidColorBrush(finalColor);
        }
    }

    public class Column : ViewModelBase
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();
        public int TaskCount => Tasks.Count;
    }

    public class KanbanBoardViewModel : ViewModelBase
    {
        public ObservableCollection<Column> Columns { get; set; } = new();

        private TaskItem? _selectedTask;
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDetailPanelOpen));
            }
        }

        public bool IsDetailPanelOpen => SelectedTask != null;

        public KanbanBoardViewModel()
        {
            LoadData();
        }

        public void CloseDetails()
        {
            SelectedTask = null;
        }

        private void LoadData()
        {
            var col1 = new Column { Id = "col-1", Title = "Backlog" };
            col1.Tasks.Add(new TaskItem { Id = "1", Content = "Design System Audit", Tag = "Design", Priority = "High", Date = "Tomorrow" });
            col1.Tasks.Add(new TaskItem { Id = "2", Content = "Q3 Marketing Assets", Tag = "Marketing", Priority = "Medium", Date = "Oct 24" });
            col1.Tasks.Add(new TaskItem { Id = "3", Content = "Update dependencies", Tag = "Dev", Priority = "Low", Date = "Oct 28" });

            var col2 = new Column { Id = "col-2", Title = "In Progress" };
            col2.Tasks.Add(new TaskItem { Id = "4", Content = "Dark Mode Implementation", Tag = "Dev", Priority = "High", Date = "Today" });

            var col3 = new Column { Id = "col-3", Title = "Review" };

            var col4 = new Column { Id = "col-4", Title = "Complete" };
            col4.Tasks.Add(new TaskItem { Id = "5", Content = "Competitor Analysis", Tag = "Product", Priority = "Medium", Date = "Oct 10" });

            Columns.Add(col1);
            Columns.Add(col2);
            Columns.Add(col3);
            Columns.Add(col4);
        }
    }
}