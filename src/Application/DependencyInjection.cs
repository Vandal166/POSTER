using Application.Contracts;
using Application.Contracts.Persistence;
using Application.Services;
using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly);
        
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IPostLikeService, PostLikeService>();
        services.AddScoped<IPostViewService, PostViewService>();
        services.AddScoped<ICommentLikeService, CommentLikeService>();
        services.AddScoped<IPostCommentService, PostCommentService>();

        services.AddScoped<IConversationService, ConversationService>();

        services.AddSingleton<IBlobService, BlobService>();
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));
        
        return services;
    }
}