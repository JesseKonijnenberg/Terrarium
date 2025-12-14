using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using Terrarium.Avalonia.ViewModels;
using Velopack;
using Velopack.Sources;

namespace Terrarium.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateMyApp();
        }

        private void Window_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }

        private void ResizeGrip_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginResizeDrag(WindowEdge.SouthEast, e);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void UpdateMyApp()
        {
            try
            {
                var source = new GithubSource("https://github.com/JesseKonijnenberg/Terrarium", null, false);
                var mgr = new UpdateManager(source);

                if (!mgr.IsInstalled)
                    return;

                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                    return;

                await Dispatcher.UIThread.InvokeAsync(() => ShowUpdateUI(mgr, newVersion));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update failed: {ex.Message}");
            }
        }

        private async void ShowUpdateUI(UpdateManager mgr, UpdateInfo newVersion)
        {
            var dialog = new Window
            {
                Title = "Terrarium Update",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                SystemDecorations = SystemDecorations.Full,
                Background = Brushes.White
            };

            var panel = new StackPanel
            {
                Margin = new global::Avalonia.Thickness(20),
                Spacing = 15,
                VerticalAlignment = VerticalAlignment.Center
            };

            // --- TEXT ---
            var titleText = new TextBlock
            {
                Text = "New Version Available!",
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeight.Bold,
                FontSize = 16
            };

            var statusText = new TextBlock
            {
                Text = $"Version {newVersion.TargetFullRelease.Version} is ready.\nUpdate now?",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(titleText);
            panel.Children.Add(statusText);

            // --- PROGRESS BAR (Hidden initially) ---
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Height = 20,
                IsVisible = false, // Hide until they click Yes
                Margin = new global::Avalonia.Thickness(0, 10, 0, 0)
            };
            panel.Children.Add(progressBar);

            // --- BUTTONS ---
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 20,
                Margin = new global::Avalonia.Thickness(0, 15, 0, 0)
            };

            var btnYes = new Button { Content = "Download & Restart", Background = Brushes.LightGreen };
            var btnNo = new Button { Content = "Cancel" };

            buttonPanel.Children.Add(btnYes);
            buttonPanel.Children.Add(btnNo);
            panel.Children.Add(buttonPanel);

            // --- LOGIC ---

            btnNo.Click += (_, _) =>
            {
                dialog.Close();
            };

            btnYes.Click += async (_, _) =>
            {
                // Update UI state
                btnYes.IsEnabled = false;
                btnNo.IsEnabled = false;
                buttonPanel.IsVisible = false;
                progressBar.IsVisible = true;
                statusText.Text = "Downloading update...";

                try
                {
                    await mgr.DownloadUpdatesAsync(newVersion, (progress) =>
                    {
                        // Update bar on UI thread
                        Dispatcher.UIThread.Post(() => progressBar.Value = progress);
                    });

                    statusText.Text = "Installing...";

                    mgr.ApplyUpdatesAndRestart(newVersion);
                }
                catch (Exception ex)
                {
                    statusText.Text = "Error!";
                    System.Diagnostics.Debug.WriteLine(ex);
                    dialog.Close();
                }
            };

            dialog.Content = panel;
            await dialog.ShowDialog(this);
        }
    }
}