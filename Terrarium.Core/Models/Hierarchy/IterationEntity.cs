namespace Terrarium.Core.Models.Hierarchy;

public class IterationEntity : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string ProjectId { get; set; } = string.Empty;
    public virtual ProjectEntity Project { get; set; } = null!;
}