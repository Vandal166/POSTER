using Application.Contracts;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        
        //services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostLikeService, PostLikeService>();
        services.AddScoped<IPostCommentService, PostCommentService>();
        
        
        return services;
    }
}