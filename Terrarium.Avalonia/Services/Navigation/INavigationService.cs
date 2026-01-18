using System.ComponentModel;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.Services.Navigation;

public interface INavigationService : INotifyPropertyChanged
{
    // The top-level "Frame" (Landing, WorkspaceA, WorkspaceB, Login)
    ViewModelBase? RootState { get; }
    
    // The current active page inside the RootState
    ViewModelBase? CurrentContent { get; }

    // Navigation methods
    void SetRoot<T>() where T : ViewModelBase;
    void NavigateContent<T>(bool clearHistory = false) where T : ViewModelBase;
    
    void GoBack();
    bool CanGoBack { get; }
}