using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    internal class DemoDataSeeder : IDataSeeder
    {
        public Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            // Add demo data when needed for development or testing
            return Task.CompletedTask;
        }
    }
}
