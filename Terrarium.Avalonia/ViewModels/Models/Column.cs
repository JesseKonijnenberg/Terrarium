using System.Collections.ObjectModel;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels.Models
{
    public class Column : ViewModelBase
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public ObservableCollection<TaskItem> Tasks { get; set; } = new();
        public int TaskCount => Tasks.Count;

        public void AddTask(TaskItem task)
        {
            if (!Tasks.Contains(task))
            {
                Tasks.Add(task);
                OnPropertyChanged(nameof(TaskCount));
            }
        }

        public void RemoveTask(TaskItem task)
        {
            if (Tasks.Contains(task))
            {
                Tasks.Remove(task);
                OnPropertyChanged(nameof(TaskCount));
            }
        }
    }
}