using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Terrarium.Avalonia.ViewModels;

namespace Terrarium.Avalonia.Controls
{
    public partial class KanbanColumnControl : UserControl
    {
        public KanbanColumnControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains("TaskItem"))
            {
                e.DragEffects = DragDropEffects.Move;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            var task = e.Data.Get("TaskItem") as TaskItem;
            var targetColumn = DataContext as Column;

            if (task != null && targetColumn != null)
            {
                var window = this.FindAncestorOfType<Window>();
                if (window?.DataContext is KanbanBoardViewModel mainVm)
                {
                    mainVm.MoveTask(task, targetColumn);
                }
            }
        }
    }
}