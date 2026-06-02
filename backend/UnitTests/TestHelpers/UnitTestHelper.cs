using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Settings;

namespace UnitTests.TestHelpers;

public static class UnitTestHelper
{
    public static DbContextOptions<ApplicationDbContext> CreateInMemoryDatabaseOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public static IOptions<JwtSettings> CreateJwtSettingsOptions()
    {
        return Options.Create(new JwtSettings
        {
            Secret = "test-secret-key-1234567890-test-1234567890-extended",
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
    }
}
