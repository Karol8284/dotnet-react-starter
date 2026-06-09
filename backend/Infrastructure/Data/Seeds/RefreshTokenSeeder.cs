using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    internal class RefreshTokenSeeder : IDataSeeder
    {
        public Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            // Add refresh token seed data if needed
            return Task.CompletedTask;
        }
    }
}
