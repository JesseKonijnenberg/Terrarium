using System.ComponentModel.DataAnnotations;

namespace Terrarium.Core.Models;

public abstract class EntityBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Metadata for Sync & Conflict Resolution
    public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
    
    // Soft Delete (Critical for syncing multiple devices)
    public bool IsDeleted { get; set; } = false;
    
    [ConcurrencyCheck] 
    public int Version { get; set; }
}