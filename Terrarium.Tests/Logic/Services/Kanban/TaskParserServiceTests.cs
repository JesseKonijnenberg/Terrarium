using Terrarium.Core.Enums.Kanban;
using Terrarium.Logic.Services.Kanban;

namespace Terrarium.Tests.Logic.Services.Kanban;

public class TaskParserServiceTests
{
    private readonly TaskParserService _parser = new();

    [Fact]
    public void Parse_HeaderWithEmoji_ShouldCleanColumnName()
    {
        var text = "## 🏔️ Backlog\n- [ ] Test Task";
        
        var result = _parser.ParseClipboardText(text).First();

        Assert.Equal("Backlog", result.TargetColumnName);
    }

    [Fact]
    public void Parse_TaskWithMetadata_ShouldExtractTagAndPriority()
    {
        var text = "## In Progress\n- [ ] Implement Auth #Security !High";
        
        var result = _parser.ParseClipboardText(text).First();

        Assert.Equal("Implement Auth", result.Title);
        Assert.Equal("Security", result.Tag);
        Assert.Equal(TaskPriority.High, result.Priority);
    }

    [Fact]
    public void Parse_MultiLineDescription_ShouldAppendWithNewLines()
    {
        var text = "## Review\n- [ ] Task Title\n> Line 1\n> Line 2";
        
        var result = _parser.ParseClipboardText(text).First();

        Assert.Contains("Line 1", result.Description);
        Assert.Contains("Line 2", result.Description);
        Assert.Contains(Environment.NewLine, result.Description);
    }

    [Fact]
    public void Parse_EmptyText_ShouldReturnEmptyEnumerable()
    {
        var result = _parser.ParseClipboardText("");

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_MixedContent_ShouldIdentifyCorrectColumnsForEachTask()
    {
        var text = """
                   ## Backlog
                   - [ ] Task A
                   ## Done
                   - [ ] Task B
                   """;

        var results = _parser.ParseClipboardText(text).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Backlog", results[0].TargetColumnName);
        Assert.Equal("Task A", results[0].Title);
        Assert.Equal("Done", results[1].TargetColumnName);
        Assert.Equal("Task B", results[1].Title);
    }

    [Theory]
    [InlineData("!med", TaskPriority.Medium)]
    [InlineData("!medium", TaskPriority.Medium)]
    [InlineData("!high", TaskPriority.High)]
    [InlineData("!random", TaskPriority.Low)]
    public void ParsePriority_ShouldHandleVariousInputs(string input, TaskPriority expected)
    {
        var text = $"## Backlog\n- [ ] Task {input}";
        
        var result = _parser.ParseClipboardText(text).First();

        Assert.Equal(expected, result.Priority);
    }

    [Fact]
    public void Parse_IgnoreMetadataLines_ShouldSkipExportsAndDividers()
    {
        var text = """
                   ---
                   ## Backlog
                   - [ ] Valid Task
                   [[ Last Export ]]
                   """;

        var results = _parser.ParseClipboardText(text).ToList();

        Assert.Single(results);
        Assert.Equal("Valid Task", results[0].Title);
    }
}