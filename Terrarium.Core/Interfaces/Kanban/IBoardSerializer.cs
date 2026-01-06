using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban
{
    public interface IBoardSerializer
    {
        string ToMarkdown(IEnumerable<ColumnEntity> columns);
    }  
}

