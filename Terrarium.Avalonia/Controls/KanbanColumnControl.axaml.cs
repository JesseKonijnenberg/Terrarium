using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.ViewModels.Models;

namespace Terrarium.Avalonia.Controls
{
    public partial class KanbanColumnControl : UserControl
    {
        public KanbanColumnControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}