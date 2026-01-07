using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Terrarium.Avalonia.Behaviors
{
    public class BoardInteractions : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> FocusOnClickProperty =
            AvaloniaProperty.RegisterAttached<BoardInteractions, Control, bool>("FocusOnClick");
        public static void SetFocusOnClick(Control element, bool value) => element.SetValue(FocusOnClickProperty, value);
        public static bool GetFocusOnClick(Control element) => element.GetValue(FocusOnClickProperty);
        
        public static readonly AttachedProperty<bool> StopPropagationProperty =
            AvaloniaProperty.RegisterAttached<BoardInteractions, Control, bool>("StopPropagation");
        public static void SetStopPropagation(Control element, bool value) => element.SetValue(StopPropagationProperty, value);
        public static bool GetStopPropagation(Control element) => element.GetValue(StopPropagationProperty);
        
        public static readonly AttachedProperty<ICommand?> CommandOnPressedProperty =
            AvaloniaProperty.RegisterAttached<BoardInteractions, Control, ICommand?>("CommandOnPressed");
        public static void SetCommandOnPressed(Control element, ICommand? value) => element.SetValue(CommandOnPressedProperty, value);
        public static ICommand? GetCommandOnPressed(Control element) => element.GetValue(CommandOnPressedProperty);
        
        public static readonly AttachedProperty<ICommand?> CommandOnDoubleTapProperty =
            AvaloniaProperty.RegisterAttached<BoardInteractions, Control, ICommand?>("CommandOnDoubleTap");
        public static void SetCommandOnDoubleTap(Control element, ICommand? value) => element.SetValue(CommandOnDoubleTapProperty, value);
        public static ICommand? GetCommandOnDoubleTap(Control element) => element.GetValue(CommandOnDoubleTapProperty);
        
        public static readonly AttachedProperty<object?> CommandParameterProperty =
            AvaloniaProperty.RegisterAttached<BoardInteractions, Control, object?>("CommandParameter");
        public static void SetCommandParameter(Control element, object? value) => element.SetValue(CommandParameterProperty, value);
        public static object? GetCommandParameter(Control element) => element.GetValue(CommandParameterProperty);

        
        static BoardInteractions()
        {
            FocusOnClickProperty.Changed.AddClassHandler<Control>((sender, args) => 
                HandlePointerPressedHook(sender));

            StopPropagationProperty.Changed.AddClassHandler<Control>((sender, args) => 
                HandlePointerPressedHook(sender));

            CommandOnPressedProperty.Changed.AddClassHandler<Control>((sender, args) => 
                HandlePointerPressedHook(sender));
            
            CommandOnDoubleTapProperty.Changed.AddClassHandler<Control>((sender, args) =>
            {
                var oldCmd = args.OldValue as ICommand;
                var newCmd = args.NewValue as ICommand;
                
                if (oldCmd == null && newCmd != null)
                {
                    sender.DoubleTapped += OnDoubleTapped;
                }
                else if (oldCmd != null && newCmd == null)
                {
                    sender.DoubleTapped -= OnDoubleTapped;
                }
            });
        }

        private static void HandlePointerPressedHook(Control control)
        {
            control.PointerPressed -= OnPointerPressed;
            
            bool needsHook = GetFocusOnClick(control) || 
                             GetStopPropagation(control) || 
                             GetCommandOnPressed(control) != null;

            if (needsHook)
            {
                control.PointerPressed += OnPointerPressed;
            }
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Control control) return;
            
            if (GetFocusOnClick(control))
            {
                control.Focus();
            }
            
            var command = GetCommandOnPressed(control);
            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
            }
            
            if (GetStopPropagation(control))
            {
                e.Handled = true;
            }
        }

        private static void OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (sender is not Control control) return;

            var command = GetCommandOnDoubleTap(control);
            var parameter = GetCommandParameter(control) ?? control.DataContext;

            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }
}