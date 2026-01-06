using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Terrarium.Avalonia.ViewModels;

namespace Terrarium.Avalonia.Views
{
    public partial class KanbanBoardView : UserControl
    {
        public KanbanBoardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void OnBackgroundPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var grid = sender as Grid;
            grid?.Focus();
    
            if (DataContext is KanbanBoardViewModel vm)
            {
                vm.DeselectAllCommand.Execute(null);
            }
        }
    }
}