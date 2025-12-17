using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Terrarium.Avalonia.Views
{
    public partial class GardenView : UserControl
    {
        public GardenView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}