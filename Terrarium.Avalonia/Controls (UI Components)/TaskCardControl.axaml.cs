using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Terrarium.Avalonia.Models.Kanban;
using Terrarium.Avalonia.ViewModels;

namespace Terrarium.Avalonia.Controls__UI_Components_
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
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true; 
        }
        
        private void OnCardDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (DataContext is TaskItem task)
            {
                Control? current = this;
                KanbanBoardViewModel? vm = null;

                while (current != null)
                {
                    if (current.DataContext is KanbanBoardViewModel foundVm)
                    {
                        vm = foundVm;
                        break;
                    }
                    current = current.GetVisualParent() as Control;
                }

                if (vm != null)
                {
                    vm.OpenTaskCommand.Execute(task);
                    e.Handled = true;
                }
            }
        }
    }
}