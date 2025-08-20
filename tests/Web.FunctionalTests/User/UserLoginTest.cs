using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace Application.IntegrationTests.User;

public class UserLoginTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;

    private readonly RegisteredUserFixture _userFixture;

    public UserLoginTest(WebApplicationFactory<Program> registeredUser)
    {
        _userFixture = new RegisteredUserFixture(registeredUser);
    }
    
    [Fact]
    public async Task LoginUser_ShouldSucceedWithRedirectToCompleteProfile_WhenValidData()
    {
        var baseUrl = "http://localhost:5291";

        // fill login form
        await _page.GotoAsync($"{baseUrl}/Account/Login");
        await _page.FillAsync("input[name='Dto.Login']", _userFixture.Email);
        await _page.FillAsync("input[name='Dto.Password']", _userFixture.Password);
        await _page.ClickAsync("button[type='submit']");

        await _page.WaitForURLAsync($"{baseUrl}/Account/CompleteProfile");

        // Assert
        Assert.Contains("Complete Profile", await _page.TitleAsync());
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