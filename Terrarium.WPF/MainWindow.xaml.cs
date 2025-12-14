using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Terrarium.WPF.ViewModels;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Terrarium.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(PlantViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        // MainWindow.xaml.cs

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point pos = e.GetPosition(this);

                double edgeMargin = 10;

                bool isLeftEdge = pos.X <= edgeMargin;
                bool isRightEdge = pos.X >= this.ActualWidth - edgeMargin;
                bool isTopEdge = pos.Y <= edgeMargin;
                bool isBottomEdge = pos.Y >= this.ActualHeight - edgeMargin;

                if (!isLeftEdge && !isRightEdge && !isTopEdge && !isBottomEdge)
                {
                    this.DragMove();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // MAGIC RESIZE CODE STARTS HERE

        // This defines the "SendMessage" function from the Windows System (user32.dll)
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // This event fires when you click the Resize Grip
        private void ResizeGrip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Stop any other WPF mouse processing
                e.Handled = true;

                // Get the "Handle" (ID) of your window
                WindowInteropHelper helper = new WindowInteropHelper(this);

                // Send the "Resize Bottom Right" system command
                // WM_SYSCOMMAND = 0x112
                // SC_SIZE = 0xF000
                // HTBOTTOMRIGHT = 17 (This tells Windows to drag the bottom-right corner)
                SendMessage(helper.Handle, 0x112, (IntPtr)(0xF000 + 17), IntPtr.Zero);
            }
        }
    }
}