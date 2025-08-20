using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Application.IntegrationTests.User;

public class UserCompleteProfileTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;

    private readonly RegisteredUserFixture _userFixture;

    public UserCompleteProfileTest(WebApplicationFactory<Program> registeredUser)
    {
        _userFixture = new RegisteredUserFixture(registeredUser);
    }
    
    [Fact]
    public async Task CompleteProfile_ShouldSucceedWithRedirectToIndex_WhenValidData()
    {
        var baseUrl = "http://localhost:5291";

        await _page.GotoAsync($"{baseUrl}/Account/Login");
        await _page.FillAsync("input[name='Dto.Login']", _userFixture.Email);
        await _page.FillAsync("input[name='Dto.Password']", _userFixture.Password);
        await _page.ClickAsync("button[type='submit']");

        // after login, wait for CompleteProfile page
        await _page.WaitForURLAsync($"{baseUrl}/Account/CompleteProfile");
        
        await _page.WaitForSelectorAsync("input[name='Dto.Username']", new() { State = WaitForSelectorState.Visible });

        await _page.FillAsync("input[name='Dto.Username']", _userFixture.Username);
        await _page.ClickAsync("button[type='submit']");
        
        await _page.WaitForURLAsync($"{baseUrl}/");
        
        // Assert
        Assert.Contains("Home", await _page.TitleAsync());
    }
    
     
    public async Task InitializeAsync()
    {
        await _userFixture.InitializeAsync();
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