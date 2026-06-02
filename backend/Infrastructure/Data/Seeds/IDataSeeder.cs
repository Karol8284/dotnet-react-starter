using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data;

namespace Infrastructure.Data.Seeds
{
    internal interface IDataSeeder
    {
        Task SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default);
    }
}
