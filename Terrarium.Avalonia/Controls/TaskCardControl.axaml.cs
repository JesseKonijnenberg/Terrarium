using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.ViewModels.Models;
using System;
using System.Threading.Tasks;

namespace Terrarium.Avalonia.Controls
{
    public partial class TaskCardControl : UserControl
    {
        private Point _dragStartPoint;
        private bool _isDragging;

        public TaskCardControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed)
            {
                _dragStartPoint = e.GetPosition(null);
                _isDragging = false;
            }
        }

        private async void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (!point.Properties.IsLeftButtonPressed) return;
            if (_isDragging) return;

            var currentPoint = e.GetPosition(null);
            var deltaX = Math.Abs(currentPoint.X - _dragStartPoint.X);
            var deltaY = Math.Abs(currentPoint.Y - _dragStartPoint.Y);

            if (deltaX > 10 || deltaY > 10)
            {
                _isDragging = true;
                await StartDrag(e);
            }
        }

        private async Task StartDrag(PointerEventArgs e)
        {
            if (DataContext is not TaskItem task) return;

            var dragData = new DataObject();
            dragData.Set("TaskItem", task);

            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);

            _isDragging = false;
        }

        private void OnTapped(object? sender, TappedEventArgs e)
        {
            if (_isDragging) return;
            if (DataContext is not TaskItem taskItem) return;

            if (VisualRoot is Window window && window.DataContext is KanbanBoardViewModel mainVm)
            {
                if (mainVm.SelectTaskCommand.CanExecute(taskItem))
                {
                    mainVm.SelectTaskCommand.Execute(taskItem);
                }
            }
        }
    }
}