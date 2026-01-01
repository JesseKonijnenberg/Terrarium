using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Terrarium.Avalonia.Controls
{
    public partial class TopBarControl : UserControl
    {
        public TopBarControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}