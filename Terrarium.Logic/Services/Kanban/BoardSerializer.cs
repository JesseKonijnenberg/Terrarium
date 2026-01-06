using Terrarium.Core.Interfaces.Kanban;
using Terrarium.Core.Models.Kanban;
using Terrarium.Logic.Services.Kanban.Strategies;

namespace Terrarium.Logic.Services.Kanban;

public class BoardSerializer : IBoardSerializer
{
    private IBoardFormattingStrategy _strategy;
    
    public BoardSerializer(string templatePath)
    {
        _strategy = new TemplateMarkdownStrategy(templatePath);
    }

    public void SetStrategy(IBoardFormattingStrategy newStrategy)
    {
        _strategy = newStrategy;
    }

    public string ToMarkdown(IEnumerable<ColumnEntity> columns)
    {
        return _strategy.Serialize(columns);
    }
}