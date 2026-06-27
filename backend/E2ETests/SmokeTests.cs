using System.Net;

namespace E2ETests;

public class SmokeTests
{
    private static readonly Uri BackendBaseUri = CreateUri("SMOKE_API_URL", "http://localhost:5000");
    private static readonly Uri FrontendBaseUri = CreateUri("SMOKE_FRONTEND_URL", "http://localhost:3000");

    [Fact]
    public async Task Backend_health_endpoint_returns_success()
    {
        using var client = CreateClient(BackendBaseUri);

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Backend_protected_auth_endpoint_rejects_anonymous_request()
    {
        using var client = CreateClient(BackendBaseUri);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Frontend_root_returns_html_shell()
    {
        using var client = CreateClient(FrontendBaseUri);

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("<div id=\"root\"></div>", html, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpClient CreateClient(Uri baseUri)
    {
        return new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    private static Uri CreateUri(string variableName, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return new Uri(string.IsNullOrWhiteSpace(value) ? fallback : value, UriKind.Absolute);
    }
}