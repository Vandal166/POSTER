using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Application.IntegrationTests.User;

public class UserRegisterTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;
    
    [Fact] // Not mocking, we are actually registering a user that gets saved in Keycloak_db
    public async Task RegisterUser_ShouldSucceedWithRedirectToLogin_WhenValidData()
    {
        var baseUrl = "http://localhost:5291";
        var email = $"testuser{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        var pwd  = "TestPassword123!";
        
        await _page.GotoAsync($"{baseUrl}/Account/Register");
        await _page.FillAsync("input[name='Dto.Email']", email);
        await _page.FillAsync("input[name='Dto.Password']", pwd);
        await _page.ClickAsync("button[type='submit']");
        
        // Wait for registration to complete and redirection to login page
        await _page.WaitForURLAsync($"{baseUrl}/Account/Login");
        
        // Assert
        Assert.Contains("Login", await _page.TitleAsync());
    }
    
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // change to false to see the browser
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });

        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    public void Dispose()
    {
        _page?.CloseAsync().Wait();
        _context?.CloseAsync().Wait();
    }
}