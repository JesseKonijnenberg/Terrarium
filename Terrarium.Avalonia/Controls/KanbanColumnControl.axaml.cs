using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;
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
            // Allow drop only if the item is a TaskItem
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
            var taskToMove = e.Data.Get("TaskItem") as TaskItem;
            var targetColumn = DataContext as Column;

            if (taskToMove == null || targetColumn == null) return;

            // 1. Find the Main Window ViewModel to execute the move/reorder command
            var window = this.FindAncestorOfType<Window>();
            var mainVm = window?.DataContext as KanbanBoardViewModel;
            if (mainVm == null) return;

            // 2. Determine the insertion index
            var itemsControl = this.FindDescendantOfType<ItemsControl>();
            if (itemsControl?.Presenter?.Panel is not StackPanel stackPanel) return;

            // Get the relative drop position within the column
            var positionInStackPanel = e.GetPosition(stackPanel);

            int insertionIndex = targetColumn.Tasks.Count;

            // Iterate through the visible item containers (the TaskCardControls)
            for (int i = 0; i < stackPanel.Children.Count; i++)
            {
                var container = stackPanel.Children[i] as Control;
                if (container == null) continue;

                // Check if the drop position is above the current item's midpoint
                var relativePosition = e.GetPosition(container);
                if (relativePosition.Y < container.Bounds.Height / 2)
                {
                    insertionIndex = i;
                    break;
                }
            }

            // 3. Execute the Reorder or Move logic
            var sourceColumn = mainVm.Columns.FirstOrDefault(c => c.Tasks.Contains(taskToMove));

            if (sourceColumn == targetColumn)
            {
                // Reorder within the same column
                int oldIndex = sourceColumn.Tasks.IndexOf(taskToMove);
                if (oldIndex != insertionIndex)
                {
                    // If moving down, adjust index for the removal
                    if (oldIndex < insertionIndex)
                    {
                        insertionIndex--;
                    }
                    sourceColumn.Tasks.Move(oldIndex, insertionIndex);
                }
            }
            else
            {
                // Move to a different column (Cross-column drag)
                sourceColumn?.RemoveTask(taskToMove);
                targetColumn.Tasks.Insert(insertionIndex, taskToMove);
            }
        }
    }
}