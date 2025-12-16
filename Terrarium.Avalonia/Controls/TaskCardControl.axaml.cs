using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.ViewModels.Models;

namespace Terrarium.Avalonia.Controls
{
    public partial class TaskCardControl : UserControl
    {
        public TaskCardControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            var task = DataContext as TaskItem;
            if (task == null) return;

            var dragData = new DataObject();
            dragData.Set("TaskItem", task);

            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            var window = this.FindAncestorOfType<Window>();
            if (window?.DataContext is KanbanBoardViewModel mainVm)
            {
                var task = DataContext as TaskItem;
                if (task != null)
                {
                    mainVm.SelectedTask = task;
                }
            }
        }
    }
}