using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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
            this.Focus();
        }
    }
}