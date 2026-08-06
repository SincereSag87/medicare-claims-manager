using System.Net;

namespace MedicareClaimsManager.Tests;

public class RouteSmokeTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public RouteSmokeTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/Patients")]
    [InlineData("/Providers")]
    [InlineData("/Claims")]
    [InlineData("/Reports")]
    [InlineData("/Admin")]
    public async Task SecuredRoutes_RedirectAnonymousUsersToLogin(string route)
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.OriginalString);
    }
}
