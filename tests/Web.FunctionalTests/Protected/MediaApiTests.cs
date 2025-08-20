using Microsoft.Playwright;

namespace Web.FunctionalTests.Protected;

public class MediaApiTests : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IAPIRequestContext _api;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _api = await _playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = "http://localhost:5291"
        });
    }

    public async Task DisposeAsync()
    {
        await _api.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task UploadImage_ShouldFail_WhenUnauthenticated()
    {
        // Create multipart form data using Playwright's API
        var formData = new Dictionary<string, object>
        {
            ["file"] = new FilePayload
            {
                Name = "/wwwroot/uploads/content-not-found.png",
                MimeType = "image/png",
                Buffer = new byte[1024]
            }
        };

        // Send POST request without authentication
        var response = await _api.PostAsync("images", new APIRequestContextOptions
        {
            Params = formData
        });

        Assert.Equal("http://localhost:5291/Account/Login", response.Url);
    }

    [Fact]
    public async Task DeleteImage_ShouldFail_WhenUnauthenticated()
    {
        // Create a random GUID for the test
        var randomGuid = Guid.NewGuid();

        // Send DELETE request without authentication
        var response = await _api.DeleteAsync($"images/{randomGuid}");

        Assert.Equal("http://localhost:5291/Account/Login", response.Url);
    }
}