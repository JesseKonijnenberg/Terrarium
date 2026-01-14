using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Terrarium.Avalonia.Views;

public partial class LandingView : UserControl
{
    public LandingView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}