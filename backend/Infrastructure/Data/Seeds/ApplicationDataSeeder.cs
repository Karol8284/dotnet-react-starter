using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    public class ApplicationDataSeeder : IDataSeeder
    {
        public Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            // Add application-level seed data if required
            return Task.CompletedTask;
        }
    }
}
