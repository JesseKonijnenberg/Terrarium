using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Models;
using Terrarium.Core.Models.Data;
using Terrarium.Data;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;
using Terrarium.Logic.Services;

namespace Terrarium.Avalonia
{
    public partial class App : Application
    {
        public IServiceProvider? Services { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var collection = new ServiceCollection();
            ConfigureServices(collection);
            Services = collection.BuildServiceProvider();
            using (var scope = Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TerrariumDbContext>();
                DbInitializer.Initialize(dbContext);
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();

                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var storageOptions = new StorageOptions();
            services.AddSingleton(storageOptions);

            services.AddTerrariumData(storageOptions);

            services.AddSingleton<IBoardRepository, SqliteBoardRepository>();
            services.AddSingleton<IBoardService, BoardService>();
            services.AddSingleton<IGardenService, GardenService>();

            services.AddSingleton<IGardenEconomyService, GardenEconomyService>();

            services.AddSingleton<KanbanBoardViewModel>();
            services.AddSingleton<GardenViewModel>();
            services.AddSingleton<MainWindowViewModel>();
        }
    }
}