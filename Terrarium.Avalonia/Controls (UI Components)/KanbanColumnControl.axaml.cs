using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Terrarium.Avalonia.Controls;

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