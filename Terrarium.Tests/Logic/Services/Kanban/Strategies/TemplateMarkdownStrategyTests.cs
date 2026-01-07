using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services.Kanban.Strategies;

namespace Terrarium.Tests.Logic.Services.Kanban.Strategies;

public class TemplateMarkdownStrategyTests : IDisposable
{
    private readonly string _tempPath;

    public TemplateMarkdownStrategyTests()
    {
        _tempPath = Path.GetTempFileName();
        var template = """
            # Board Export
            [[COLUMN_START]]
            ## {{ColumnName}}
            [[TASK_START]]
            ### {{TaskTitle}}
            [[DESC_START]]
            {{TaskDescription}}
            [[DESC_END]]
            [[TASK_END]]
            [[COLUMN_END]]
            """;
        File.WriteAllText(_tempPath, template);
    }

    [Fact]
    public void Serialize_MultiLineDescription_ShouldAddBlockquoteToEveryLine()
    {
        var strategy = new TemplateMarkdownStrategy(_tempPath);
        var columns = new List<ColumnEntity> {
            new() { Title = "Col", Tasks = new List<TaskEntity> {
                new() { Title = "T1", Description = "Line 1\nLine 2" }
            }}
        };

        var result = strategy.Serialize(columns);

        // We expect:
        // > Line 1
        // > Line 2
        Assert.Contains("> Line 1", result);
        Assert.Contains("> Line 2", result);
    }

    [Fact]
    public void Serialize_EmptyColumn_ShouldStillRenderColumnHeader()
    {
        var strategy = new TemplateMarkdownStrategy(_tempPath);
        var columns = new List<ColumnEntity> {
            new() { Title = "Empty Column", Tasks = new List<TaskEntity>() }
        };

        var result = strategy.Serialize(columns);

        Assert.Contains("## Empty Column", result);
        // Ensure no task header was rendered
        Assert.DoesNotContain("### ", result); 
    }

    [Fact]
    public void Serialize_MultipleEntities_ShouldRepeatBlocksCorrectly()
    {
        var strategy = new TemplateMarkdownStrategy(_tempPath);
        var columns = new List<ColumnEntity> {
            new() { Title = "C1", Tasks = new List<TaskEntity> { new() { Title = "T1" }, new() { Title = "T2" } } },
            new() { Title = "C2", Tasks = new List<TaskEntity> { new() { Title = "T3" } } }
        };

        var result = strategy.Serialize(columns);

        Assert.Contains("## C1", result);
        Assert.Contains("### T1", result);
        Assert.Contains("### T2", result);
        Assert.Contains("## C2", result);
        Assert.Contains("### T3", result);
    }

    [Fact]
    public void Serialize_MissingTemplateFile_ShouldReturnErrorMessage()
    {
        var strategy = new TemplateMarkdownStrategy("non_existent_file.md");
        
        var result = strategy.Serialize(new List<ColumnEntity>());

        Assert.Contains("Error: Template file not found", result);
    }

    [Fact]
    public void Serialize_TemplateWithNoColumnMarkers_ShouldReturnRawTemplate()
    {
        var brokenPath = Path.GetTempFileName();
        File.WriteAllText(brokenPath, "Just a plain string without markers");
        var strategy = new TemplateMarkdownStrategy(brokenPath);

        var result = strategy.Serialize(new List<ColumnEntity>());

        Assert.Equal("Just a plain string without markers", result);
        File.Delete(brokenPath);
    }

    public void Dispose()
    {
        if (File.Exists(_tempPath)) File.Delete(_tempPath);
    }
}