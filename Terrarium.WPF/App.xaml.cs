using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Terrarium.Core.Interfaces;
using Terrarium.Logic.Services;
using Terrarium.WPF.ViewModels;

namespace Terrarium.WPF
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IWeatherService, LocalWeatherService>();
            services.AddSingleton<PlantGrowthService>();

            services.AddTransient<PlantViewModel>();
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }
}