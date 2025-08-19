using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    protected readonly IServiceScope _serviceScope;
    protected readonly PosterDbContext DbContext;
    protected readonly TestDataSeeder DataSeeder;
    
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _serviceScope = factory.Services.CreateScope();
        
        DbContext = _serviceScope.ServiceProvider.GetRequiredService<PosterDbContext>();
        DataSeeder = new TestDataSeeder(DbContext);
    }

    public Task InitializeAsync()
    {
        return DataSeeder.SeedAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}