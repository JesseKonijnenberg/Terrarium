namespace Terrarium.Core.Interfaces.Data;

public interface IDatabaseSeeder
{
    /// <summary>
    /// Seeds the database with environment-specific initial data.
    /// </summary>
    void Seed();
}