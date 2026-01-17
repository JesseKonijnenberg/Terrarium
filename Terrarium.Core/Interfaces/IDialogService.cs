namespace Terrarium.Core.Interfaces;

public interface IDialogService
{
    // Returns the text entered, or null if cancelled
    Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");
    
    // Returns true if Yes/OK, false if No/Cancel
    Task<bool> ConfirmAsync(string title, string message);
    
    // Simple alert
    Task AlertAsync(string title, string message);
}