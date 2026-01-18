using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.Services.Navigation;

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<ViewModelBase> _contentHistory = new();

    [ObservableProperty]
    private ViewModelBase? _rootState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    private ViewModelBase? _currentContent;

    public bool CanGoBack => _contentHistory.Count > 0;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Changes the top-level layout (e.g., from Landing to Workspace).
    /// This usually switches whether a Sidebar is visible.
    /// </summary>
    public void SetRoot<T>() where T : ViewModelBase
    {
        var root = _serviceProvider.GetRequiredService<T>();
        RootState = root;
    }

    /// <summary>
    /// Changes the content page inside the current RootState.
    /// </summary>
    public void NavigateContent<T>(bool clearHistory = false) where T : ViewModelBase
    {
        var destination = _serviceProvider.GetRequiredService<T>();

        if (clearHistory)
        {
            _contentHistory.Clear();
        }
        else if (CurrentContent != null)
        {
            _contentHistory.Push(CurrentContent);
        }

        CurrentContent = destination;
    }

    /// <summary>
    /// Returns to the previous content page.
    /// </summary>
    public void GoBack()
    {
        if (_contentHistory.Count > 0)
        {
            CurrentContent = _contentHistory.Pop();
        }
    }
}