using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Terrarium.Avalonia.Services;
using Terrarium.Avalonia.Services.Navigation;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;
using Terrarium.Core.Interfaces;
using Terrarium.Core.Interfaces.Context;
using Terrarium.Core.Interfaces.Data;
using Terrarium.Core.Interfaces.Garden;
using Terrarium.Core.Interfaces.Hierarchy;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Interfaces.Repositories;
using Terrarium.Core.Interfaces.Theming;
using Terrarium.Core.Interfaces.Update;
using Terrarium.Core.Models.Data;
using Terrarium.Data;
using Terrarium.Data.Contexts;
using Terrarium.Data.Repositories;
using Terrarium.Data.Seeding;
using Terrarium.Logic.Services.Context;
using Terrarium.Logic.Services.Hierarchy;
using Terrarium.Logic.Services.Kanban;
using Terrarium.Logic.Services.Theming;
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
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TerrariumDbContext>>();
            using var dbContext = dbContextFactory.CreateDbContext();
            
            DbInitializer.Initialize(dbContext);

            // Execute Environment-Specific Seeding
            // This will resolve to DevelopmentSeeder in DEBUG and ProductionSeeder otherwise.
            var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
            seeder.Seed();
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
        // Storage
        var storageOptions = new StorageOptions();
        services.AddSingleton(storageOptions);
        services.AddTerrariumData(storageOptions);

        // Core Services
        services.AddScoped<IHierarchyRepository, HierarchyRepository>();
        services.AddScoped<IHierarchyService, HierarchyService>();
        services.AddSingleton<IThemeRepository, ThemeRepository>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IBoardRepository, BoardRepository>();
        services.AddSingleton<IBoardService, BoardService>();
        services.AddTransient<IUpdateService, UpdateService>();
        services.AddSingleton<IBackupService, BackupService>();
        services.AddSingleton<IBoardSerializer>(new BoardSerializer(storageOptions.TemplateFilePath));
        services.AddSingleton<ITaskParserService, TaskParserService>();
        services.AddSingleton<IGardenService, GardenService>();
        services.AddSingleton<IGardenEconomyService, GardenEconomyService>();
        services.AddSingleton<IProjectContextService, ProjectContextService>();
        
        services.AddTransient<IOrganizationService, OrganizationService>();
        services.AddTransient<IWorkspaceService, WorkspaceService>();
        services.AddTransient<IProjectService, ProjectService>();
        services.AddTransient<IDialogService, DialogService>();
        
        // Repo
        services.AddTransient<IOrganizationRepository, OrganizationRepository>();
        services.AddTransient<IWorkspaceRepository, WorkspaceRepository>();
        services.AddTransient<IProjectRepository, ProjectRepository>();

        // Navigation
        services.AddSingleton<INavigationService, NavigationService>();

        // View Models
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SidebarViewModel>();
        services.AddSingleton<WorkspaceViewModel>();
        services.AddSingleton<KanbanBoardViewModel>();
        services.AddSingleton<GardenViewModel>();
        services.AddSingleton<LandingViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<UpdateViewModel>();
        services.AddSingleton<HierarchyViewModel>();
        
#if DEBUG
        services.AddScoped<IDatabaseSeeder, DevelopmentSeeder>();
#else
    services.AddScoped<IDatabaseSeeder, ProductionSeeder>();
#endif
    }
}