using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    internal class UserSeeder : IDataSeeder
    {
        public Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            // Add user seed data if needed for development or testing
            return Task.CompletedTask;
        }
    }
}
