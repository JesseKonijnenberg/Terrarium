using Terrarium.Core.Interfaces.Data;

namespace Terrarium.Data.Seeding;

public class ProductionSeeder : IDatabaseSeeder
{
    public void Seed() { /* Do nothing in production */ }
}