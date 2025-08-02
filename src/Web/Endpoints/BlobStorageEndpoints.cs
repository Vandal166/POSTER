using Application.Contracts.Persistence;

namespace Web.Endpoints;

public static class BlobStorageEndpoints
{
    public static IEndpointRouteBuilder MapBlobStorageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("videos", async (IFormFile file, IBlobService blobService) =>
        {
            // Limit size to 50MB
            if (file.Length > 35 * 1024 * 1024)
                return Results.BadRequest("File size exceeds the limit of 35MB.");
            
            await using var stream = file.OpenReadStream();
            
            var fileID = await blobService.UploadFileAsync(stream, "videos", file.ContentType);
            
            return Results.Ok(new { FileID = fileID });
            
        }).RequireAuthorization();
        
        app.MapGet("videos/{fileID:guid}", async (Guid fileID, IBlobService blobService) =>
        {
            var file = await blobService.DownloadFileAsync(fileID, "videos");
            
            if (file.ContentType is null || file.stream is null)
                return Results.NotFound();
            
            
            return Results.File(file.stream, file.ContentType);
        });

        app.MapDelete("videos/{fileID:guid}", async (Guid fileID, IBlobService blobService) =>
        {
            await blobService.DeleteFileAsync(fileID, "videos");

            return Results.NoContent();
            
        }).RequireAuthorization();
        
        
        app.MapPost("images", async (IFormFile file, IBlobService blobService) =>
        {
            // Limit size to 10MB
            if (file.Length > 10 * 1024 * 1024)
                return Results.BadRequest("File size exceeds the limit of 10MB.");
            
            await using var stream = file.OpenReadStream();
            
            var fileID = await blobService.UploadFileAsync(stream, file.ContentType, "images");
            
            return Results.Ok(new { FileID = fileID });
            
        }).RequireAuthorization();
        
        app.MapGet("images/{fileID:guid}", async (Guid fileID, IBlobService blobService) =>
        {
            var file = await blobService.DownloadFileAsync(fileID, "images");
            
            if (file.ContentType is null || file.stream is null)
                return Results.NotFound();
            
            return Results.File(file.stream, file.ContentType);
        });
        
        app.MapDelete("images/{fileID:guid}", async (Guid fileID, IBlobService blobService) =>
        {
            await blobService.DeleteFileAsync(fileID, "images");

            return Results.NoContent();
            
        }).RequireAuthorization();
        
        
        return app;
    }
}