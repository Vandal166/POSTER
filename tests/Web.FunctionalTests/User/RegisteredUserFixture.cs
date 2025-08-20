using Application.DTOs;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests.User;

// Some super simple implementation of a registered user fixture for functional tests.
// The tests are actually using the real database and services, so this is not a mock.
public class RegisteredUserFixture : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly IAuthService _authService;
    public string Username => Email.Split('@')[0];
    public string Email { get; private set; }
    public string Password { get; private set; }
    
    public RegisteredUserFixture(WebApplicationFactory<Program> factory)
    {
        _scope = factory.Services.CreateScope();
        _authService = _scope.ServiceProvider.GetRequiredService<IAuthService>();
    }
    
    public async Task InitializeAsync()
    {
        Email = $"testuser{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
        Password = "TestPassword123!";
        
        var result = await _authService.RegisterAsync(new RegisterUserDto(Email, Password), CancellationToken.None);
        if (result.IsFailed)
            throw new Exception($"Failed to register user: {string.Join(", ", result.Errors.Select(e => e.Message))}");
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}