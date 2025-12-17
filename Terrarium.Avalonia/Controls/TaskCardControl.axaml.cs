using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.ViewModels.Models;
using System;
using System.Threading.Tasks;

namespace Terrarium.Avalonia.Controls
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
    }
}