using Terrarium.Core.Enums.Kanban;
using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services.Kanban.Strategies;

namespace Terrarium.Logic.Services.Kanban;

public class BoardSerializer : IBoardSerializer
{
    private IBoardFormattingStrategy _strategy;
    private readonly string _templatePath;
    
    public BoardSerializer(string templatePath)
    {
        _templatePath = templatePath;
        _strategy = new TemplateMarkdownStrategy(templatePath); // Default to Template
    }

    /// <inheritdoc />
    public void SetFormat(SerializationFormat format)
    {
        _strategy = format switch
        {
            SerializationFormat.Template => new TemplateMarkdownStrategy(_templatePath),
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Unsupported format")
        };
    }
    
    /// <inheritdoc />
    public string ToMarkdown(IEnumerable<ColumnEntity> columns)
    {
        return _strategy.Serialize(columns);
    }
}