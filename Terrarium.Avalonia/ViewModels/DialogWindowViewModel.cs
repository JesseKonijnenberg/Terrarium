using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Terrarium.Avalonia.ViewModels.Core;

namespace Terrarium.Avalonia.ViewModels;

public partial class DialogWindowViewModel : ViewModelBase
{
    // This Action allows the ViewModel to close the Window without knowing ABOUT the Window
    public Action<bool>? CloseAction { get; set; }

    [ObservableProperty] private string _title = "Alert";
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private string _inputText = "";
    
    // Visibility Toggles
    [ObservableProperty] private bool _isInputVisible;
    [ObservableProperty] private bool _isCancelVisible = true;
    
    // Button Labels
    [ObservableProperty] private string _okButtonText = "OK";
    [ObservableProperty] private string _cancelButtonText = "Cancel";

    // Store the result here so the Service can read it after the window closes
    public bool IsConfirmed { get; private set; }

    [RelayCommand]
    private void Ok()
    {
        IsConfirmed = true;
        CloseAction?.Invoke(true); // Tells the View to close with 'True'
    }

    [RelayCommand]
    private void Cancel()
    {
        IsConfirmed = false;
        CloseAction?.Invoke(false); // Tells the View to close with 'False'
    }
}