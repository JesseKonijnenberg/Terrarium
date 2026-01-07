using Moq;
using Terrarium.Avalonia.ViewModels;
using Terrarium.Core.Interfaces.Update;

namespace Terrarium.Tests.Avalonia.ViewModels;

public class UpdateViewModelTests
{
    private readonly Mock<IUpdateService> _mockService;

    public UpdateViewModelTests()
    {
        _mockService = new Mock<IUpdateService>();
    }

    [Fact]
    public async Task CheckForUpdates_WhenVersionFound_ShouldSetIsUpdateAvailable()
    {
        _mockService.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync("1.2.3");
        
        var vm = new UpdateViewModel(_mockService.Object);
        
        await Task.Delay(100); 

        Assert.True(vm.IsUpdateAvailable);
        Assert.Equal("Update to 1.2.3", vm.UpdateButtonText);
        Assert.True(vm.ShowStartButton);
    }

    [Fact]
    public async Task ExecuteUpdate_OnSuccess_ShouldSetRestartPending()
    {
        _mockService.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync("1.2.3");
        var vm = new UpdateViewModel(_mockService.Object);
        await Task.Delay(50);
        
        _mockService.Setup(s => s.DownloadUpdatesAsync(It.IsAny<Action<int>>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        vm.UpdateCommand.Execute(null);

        Assert.False(vm.IsUpdating);
        Assert.True(vm.IsRestartPending);
        Assert.Equal("Restart Required", vm.UpdateButtonText);
    }

    [Fact]
    public async Task ExecuteUpdate_OnCancel_ShouldResetState()
    {
        _mockService.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync("1.2.3");
        var vm = new UpdateViewModel(_mockService.Object);
        await Task.Delay(50);

        _mockService.Setup(s => s.DownloadUpdatesAsync(It.IsAny<Action<int>>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new OperationCanceledException());

        vm.UpdateCommand.Execute(null);
        
        Assert.True(vm.IsCancelling);
        await Task.Delay(1100); 
        Assert.False(vm.IsCancelling);
        Assert.Contains("Update to 1.2.3", vm.UpdateButtonText);
    }

    [Fact]
    public void ExecuteRestart_ShouldInvokeService()
    {
        var vm = new UpdateViewModel(_mockService.Object);
        
        vm.RestartCommand.Execute(null);

        _mockService.Verify(s => s.ApplyUpdatesAndRestart(), Times.Once);
    }

    [Fact]
    public async Task ProgressUpdate_ShouldUpdateProgressBar()
    {
        _mockService.Setup(s => s.CheckForUpdatesAsync()).ReturnsAsync("1.2.3");
        var vm = new UpdateViewModel(_mockService.Object);
        await Task.Delay(50);

        var tcs = new TaskCompletionSource();

        _mockService.Setup(s => s.DownloadUpdatesAsync(It.IsAny<Action<int>>(), It.IsAny<CancellationToken>()))
            .Callback<Action<int>, CancellationToken>((prog, token) => 
            {
                prog(45); // Set the progress
            })
            .Returns(tcs.Task); // Return a task that is NOT finished yet
        
        vm.UpdateCommand.Execute(null);
        
        Assert.Equal(45, vm.UpdateProgress);
        Assert.Contains("45%", vm.UpdateButtonText);
        
        tcs.SetResult();
    }
}