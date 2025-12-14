using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
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

                // Arguments: Url, AccessToken (null if public), Prerelease (false)
                var source = new GithubSource("https://github.com/JesseKonijnenberg/Terrarium", null, false);
                var mgr = new UpdateManager(source);

                if (!mgr.IsInstalled)
                {
                    System.Diagnostics.Debug.WriteLine("App is not installed. Skipping check.");
                    return;
                }

                // Check for updates
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                    return;

                await mgr.DownloadUpdatesAsync(newVersion);
                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}