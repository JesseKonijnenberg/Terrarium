using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Terrarium.Avalonia.Models.Kanban;

namespace Terrarium.Avalonia.Behaviors
{
    public class KanbanDragHandler : AvaloniaObject
    {

        public static readonly AttachedProperty<bool> IsDraggableProperty =
            AvaloniaProperty.RegisterAttached<KanbanDragHandler, Control, bool>("IsDraggable");

        public static void SetIsDraggable(Control element, bool value) => element.SetValue(IsDraggableProperty, value);
        public static bool GetIsDraggable(Control element) => element.GetValue(IsDraggableProperty);

        public static readonly AttachedProperty<ICommand?> CommandProperty =
            AvaloniaProperty.RegisterAttached<KanbanDragHandler, Control, ICommand?>("Command");

        public static void SetCommand(Control element, ICommand? value) => element.SetValue(CommandProperty, value);
        public static ICommand? GetCommand(Control element) => element.GetValue(CommandProperty);

        public static readonly AttachedProperty<object?> CommandParameterProperty =
            AvaloniaProperty.RegisterAttached<KanbanDragHandler, Control, object?>("CommandParameter");

        public static void SetCommandParameter(Control element, object? value) => element.SetValue(CommandParameterProperty, value);
        public static object? GetCommandParameter(Control element) => element.GetValue(CommandParameterProperty);


        private static readonly AttachedProperty<Point> DragStartPointProperty =
            AvaloniaProperty.RegisterAttached<KanbanDragHandler, Control, Point>("DragStartPoint");

        private static readonly AttachedProperty<bool> IsDraggingProperty =
            AvaloniaProperty.RegisterAttached<KanbanDragHandler, Control, bool>("IsDragging");

        static KanbanDragHandler()
        {
            IsDraggableProperty.Changed.AddClassHandler<Control>((element, args) =>
            {
                if (args.NewValue is bool isDraggable && isDraggable)
                {
                    element.PointerPressed -= OnPointerPressed;
                    element.PointerMoved -= OnPointerMoved;
                    element.Tapped -= OnTapped;

                    element.PointerPressed += OnPointerPressed;
                    element.PointerMoved += OnPointerMoved;
                    element.Tapped += OnTapped;
                }
                else
                {
                    element.PointerPressed -= OnPointerPressed;
                    element.PointerMoved -= OnPointerMoved;
                    element.Tapped -= OnTapped;
                }
            });
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Control control)
            {
                var point = e.GetCurrentPoint(control);
                if (point.Properties.IsLeftButtonPressed)
                {
                    // Store state on the control itself
                    control.SetValue(DragStartPointProperty, e.GetPosition(null));
                    control.SetValue(IsDraggingProperty, false);
                }
            }
        }

        private static async void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is not Control control) return;

            var point = e.GetCurrentPoint(control);
            if (!point.Properties.IsLeftButtonPressed) return;

            // Check if we are already dragging
            if (control.GetValue(IsDraggingProperty)) return;

            var startPoint = control.GetValue(DragStartPointProperty);
            var currentPoint = e.GetPosition(null);

            // Calculate Distance
            var deltaX = Math.Abs(currentPoint.X - startPoint.X);
            var deltaY = Math.Abs(currentPoint.Y - startPoint.Y);

            // Threshold Check (> 10px)
            if (deltaX > 10 || deltaY > 10)
            {
                control.SetValue(IsDraggingProperty, true);
                await StartDrag(control, e);
            }
        }

        private static async Task StartDrag(Control control, PointerEventArgs e)
        {
            if (control.DataContext is not TaskItem task) return;

            var dragData = new DataObject();
            dragData.Set("TaskItem", task);

#pragma warning disable CS0618
            var result = await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
#pragma warning restore CS0618

            control.SetValue(IsDraggingProperty, false);
        }

        private static void OnTapped(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;
            
            if (control.GetValue(IsDraggingProperty)) return;

            var command = GetCommand(control);
            var parameter = GetCommandParameter(control) ?? control.DataContext;

            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
}