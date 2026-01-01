using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;

namespace Terrarium.Avalonia.Behaviors
{
    public class KanbanDropHandler : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> IsDropTargetProperty =
            AvaloniaProperty.RegisterAttached<KanbanDropHandler, Control, bool>("IsDropTarget");

        public static void SetIsDropTarget(Control element, bool value) => element.SetValue(IsDropTargetProperty, value);
        public static bool GetIsDropTarget(Control element) => element.GetValue(IsDropTargetProperty);

        static KanbanDropHandler()
        {
            IsDropTargetProperty.Changed.AddClassHandler<Control>((element, args) =>
            {
                if ((bool)args.NewValue)
                {
                    DragDrop.SetAllowDrop(element, true);
                    element.AddHandler(DragDrop.DragOverEvent, OnDragOver);
                    element.AddHandler(DragDrop.DropEvent, OnDrop);
                }
                else
                {
                    DragDrop.SetAllowDrop(element, false);
                    element.RemoveHandler(DragDrop.DragOverEvent, OnDragOver);
                    element.RemoveHandler(DragDrop.DropEvent, OnDrop);
                }
            });
        }

        private static void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains("TaskItem"))
                e.DragEffects = DragDropEffects.Move;
            else
                e.DragEffects = DragDropEffects.None;
        }

        private static void OnDrop(object? sender, DragEventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;

            // Get Data
            var taskToMove = e.Data.Get("TaskItem") as TaskItem;
            var targetColumn = control.DataContext as Column;

            if (taskToMove == null || targetColumn == null) return;

            // Find ViewModel
            var boardView = control.FindAncestorOfType<KanbanBoardView>();
            var mainVm = boardView?.DataContext as KanbanBoardViewModel;

            if (mainVm == null) return;

            // Calculate Insertion Index
            var itemsControl = control.FindDescendantOfType<ItemsControl>();
            int insertionIndex = -1;

            if (itemsControl?.Presenter?.Panel is StackPanel stackPanel)
            {
                insertionIndex = targetColumn.Tasks.Count;
                for (int i = 0; i < stackPanel.Children.Count; i++)
                {
                    var container = stackPanel.Children[i] as Control;
                    if (container == null) continue;

                    var relativePosition = e.GetPosition(container);
                    if (relativePosition.Y < container.Bounds.Height / 2)
                    {
                        insertionIndex = i;
                        break;
                    }
                }
            }
            mainVm.MoveTask(taskToMove, targetColumn, insertionIndex);
        }
    }
}