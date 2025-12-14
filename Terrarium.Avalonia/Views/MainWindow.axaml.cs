using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using Terrarium.Avalonia.ViewModels;
using Velopack;

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
                var mgr = new UpdateManager("https://github.com/JesseKonijnenberg/Terrarium");

                if (!mgr.IsInstalled)
                {
                    System.Diagnostics.Debug.WriteLine("App is not installed (likely running in Debug). Skipping update check.");
                    return;
                }

                // Check for new version
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                    return; // No update available

                // Download new version
                await mgr.DownloadUpdatesAsync(newVersion);
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update failed: {ex.Message}");
            }
        }
    }
}