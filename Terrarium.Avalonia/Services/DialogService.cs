using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Avalonia.Views;
using Terrarium.Core.Interfaces;

namespace Terrarium.Avalonia.Services;

public class DialogService : IDialogService
{
    public async Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
    {
        var vm = new DialogWindowViewModel
        {
            Title = title,
            Message = message,
            InputText = defaultValue,
            IsInputVisible = true,
            OkButtonText = "OK",
            CancelButtonText = "Cancel"
        };

        var result = await ShowWindowAsync(vm);
        
        return result ? vm.InputText : null;
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var vm = new DialogWindowViewModel
        {
            Title = title,
            Message = message,
            IsInputVisible = false,
            OkButtonText = "Yes",
            CancelButtonText = "No"
        };

        return await ShowWindowAsync(vm);
    }

    public async Task AlertAsync(string title, string message)
    {
        var vm = new DialogWindowViewModel
        {
            Title = title,
            Message = message,
            IsInputVisible = false,
            IsCancelVisible = false, // Hide cancel button
            OkButtonText = "OK"
        };

        await ShowWindowAsync(vm);
    }

    private async Task<bool> ShowWindowAsync(DialogWindowViewModel vm)
    {
        var dialog = new DialogWindow
        {
            DataContext = vm
        };

        // Wire up the Close Action so the VM can close the Window
        // This keeps the VM unaware of the specific Window class
        vm.CloseAction = (result) =>
        {
            dialog.Close(result); 
        };

        var owner = GetOwnerWindow();
        if (owner == null) return false;

        // ShowDialog returns the result passed to Close()
        var result = await dialog.ShowDialog<bool>(owner);
        return result;
    }

    private Window? GetOwnerWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
        }
        return null;
    }
}