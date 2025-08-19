using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Application.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("Poster_DB")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();
        
        
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
           var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<PosterDbContext>));
           
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<PosterDbContext>(options =>
            {
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });
        });
    }

    public async Task InitializeAsync()
    {
         await _dbContainer.StartAsync();
         
         var options = new DbContextOptionsBuilder<PosterDbContext>()
             .UseNpgsql(_dbContainer.GetConnectionString())
             .Options;

         using var context = new PosterDbContext(options);
         if ((await context.Database.GetPendingMigrationsAsync()).Any())
         {
            await context.Database.MigrateAsync();
         }
    }

    public new Task DisposeAsync()
    {
        return _dbContainer.StopAsync();
    }
}