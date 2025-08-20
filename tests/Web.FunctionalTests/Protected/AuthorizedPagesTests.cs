using Microsoft.Playwright;

namespace Web.FunctionalTests.Protected;

public class AuthorizedPagesTests : IAsyncLifetime, IDisposable
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;
    private IBrowserContext _context;
    
    [Theory]
    [InlineData("http://localhost:5291/Account/Notifications/Notifications")]
    [InlineData("http://localhost:5291/Account/Conversations/ConversationList")]
    [InlineData("http://localhost:5291/Account/CompleteProfile")]
    [InlineData("http://localhost:5291/Account/EditProfile")]
    [InlineData("http://localhost:5291/Posts/CreatePost")]
    public async Task AccessProtectedPage_ShouldRedirectToLogin_WhenNotAuthenticated(string pageUrl)
    {
        // Arrange
        
        // Act
        await _page.GotoAsync(pageUrl);

        // Assert
        Assert.Contains("Login", await _page.TitleAsync());
        Assert.Equal("http://localhost:5291/Account/Login", _page.Url);
    }
    
    [Theory]
    [InlineData("http://localhost:5291/")]
    [InlineData("http://localhost:5291/Posts/Search?")]
    [InlineData("http://localhost:5291/Account/Register")]
    public async Task AccessPublicPage_ShouldSucceed_WhenNotAuthenticated(string pageUrl)
    {
        // Arrange
        
        // Act
        await _page.GotoAsync(pageUrl);

        // Assert
        Assert.NotEqual("http://localhost:5291/Account/Login", _page.Url);
        Assert.DoesNotContain("Login", await _page.TitleAsync());
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