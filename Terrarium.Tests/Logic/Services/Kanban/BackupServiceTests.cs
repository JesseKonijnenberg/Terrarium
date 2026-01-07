using Moq;
using Terrarium.Core.Events.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Data;
using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services.Kanban;

namespace Terrarium.Tests.Logic.Services.Kanban;

public class BackupServiceTests
{
    private readonly Mock<IBoardService> _boardServiceMock = new();
    private readonly Mock<IBoardSerializer> _serializerMock = new();
    private readonly StorageOptions _options = new();

    [Fact]
    public async Task OnBoardChanged_ShouldDebounceAndOnlySaveOnce()
    {
        var service = new BackupService(_boardServiceMock.Object, _serializerMock.Object, _options);
        service.Initialize();

        _boardServiceMock.Setup(s => s.GetCachedBoard()).Returns(new List<ColumnEntity>());
        _serializerMock.Setup(s => s.ToMarkdown(It.IsAny<IEnumerable<ColumnEntity>>())).Returns("# Board Content");
        
        var args = new BoardChangedEventsArgs();
        
        _boardServiceMock.Raise(s => s.BoardChanged += null, _boardServiceMock.Object, args);
        await Task.Delay(100);
        
        _boardServiceMock.Raise(s => s.BoardChanged += null, _boardServiceMock.Object, args);
        await Task.Delay(100);
        
        _boardServiceMock.Raise(s => s.BoardChanged += null, _boardServiceMock.Object, args);
        
        _serializerMock.Verify(s => s.ToMarkdown(It.IsAny<IEnumerable<ColumnEntity>>()), Times.Never);
        
        await Task.Delay(2500);
        
        _serializerMock.Verify(s => s.ToMarkdown(It.IsAny<IEnumerable<ColumnEntity>>()), Times.Once);
        
        Assert.True(File.Exists(_options.BackupFilePath));
    }

    [Fact]
    public void Initialize_ShouldSubscribeToEvent()
    {
        var service = new BackupService(_boardServiceMock.Object, _serializerMock.Object, _options);
        
        service.Initialize();
        
        _boardServiceMock.VerifyAdd(s => s.BoardChanged += It.IsAny<EventHandler<BoardChangedEventsArgs>>(), Times.Once);
    }
}