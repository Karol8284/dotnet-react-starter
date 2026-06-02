using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UnitTests.TestHelpers;

public static class ControllerTestHelper
{
    public static ClaimsPrincipal CreateAuthenticatedUser(string userId, string email, string role = "User")
    {
        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("unique_name", email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    public static DefaultHttpContext CreateHttpContext(ClaimsPrincipal user)
    {
        return new DefaultHttpContext { User = user };
    }
}
