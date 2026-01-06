using Terrarium.Core.Models.Kanban;

namespace Terrarium.Core.Interfaces.Kanban
{
    public interface IBoardFormattingStrategy
    {
        string Serialize(IEnumerable<ColumnEntity> columns);
    } 
}

