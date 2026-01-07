using Avalonia.Controls;
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
    }
}