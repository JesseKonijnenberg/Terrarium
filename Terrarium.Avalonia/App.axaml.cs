using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Update;
using Terrarium.Core.Models.Data;
using Terrarium.Data;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;
using Terrarium.Logic.Services.Hierarchy;
using Terrarium.Logic.Services.Kanban;
using Terrarium.Logic.Services.Update;

namespace Terrarium.Avalonia;

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
            
        var backupService = Services.GetRequiredService<IBackupService>();
        backupService.Initialize();
            
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
        
        services.AddScoped<IHierarchyRepository, HierarchyRepository>();
        services.AddScoped<IHierarchyService, HierarchyService>();

        services.AddSingleton<IBoardRepository, SqliteBoardRepository>();
        services.AddSingleton<IBoardService, BoardService>();
        services.AddTransient<IUpdateService, UpdateService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IBoardSerializer>(new BoardSerializer(storageOptions.TemplateFilePath));
            
        services.AddSingleton<ITaskParserService, TaskParserService>();

        services.AddSingleton<IGardenService, GardenService>();
        services.AddSingleton<IGardenEconomyService, GardenEconomyService>();

        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<KanbanBoardViewModel>();
        services.AddSingleton<GardenViewModel>();
        services.AddSingleton<MainWindowViewModel>();
    }
}