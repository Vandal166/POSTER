using Application.Contracts;
using Application.Contracts.Auth;
using Application.Contracts.Persistence;
using Infrastructure.Auth;
using Infrastructure.Data;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PosterDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Poster_DB"));
        });
        services.AddScoped<IAdminTokenProvider, AdminTokenProvider>();
        services.AddScoped<IPasswordTokenProvider, PasswordTokenProvider>();
        services.AddScoped<IKeycloakUserService, KeycloakUserService>();
        services.AddScoped<IUserSynchronizer, UserSynchronizer>();
        services.AddScoped<ISessionValidator, SessionValidator>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IPostLikeRepository, PostLikeRepository>();
        services.AddScoped<IPostCommentRepository, PostCommentRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, BcryptHasher>();
        services.AddScoped<ITokenGenerator<HttpResponseMessage>, ROPTokenGeneratorService>();
        
        services.AddScoped<IDataSeeder, DataSeeder>();

        services.AddScoped<IKeycloakUserService, KeycloakUserService>();
        
        return services;
    }
}