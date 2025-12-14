using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;
using Terrarium.Core.Interfaces;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Services = ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = Services.GetRequiredService<MainWindow>();
                var mainViewModel = Services.GetRequiredService<PlantViewModel>();

                mainWindow.DataContext = mainViewModel;
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<IWeatherService, LocalWeatherService>();
            services.AddSingleton<PlantGrowthService>();

            services.AddTransient<PlantViewModel>();

            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }
    }
}