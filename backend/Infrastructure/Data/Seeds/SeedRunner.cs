using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    internal class SeedRunner
    {
        private readonly IEnumerable<IDataSeeder> _seeders;

        public SeedRunner(IEnumerable<IDataSeeder> seeders)
        {
            _seeders = seeders;
        }

        public async Task SeedAllAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync(dbContext, cancellationToken);
            }
        }
    }
}
