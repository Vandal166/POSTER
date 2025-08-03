using Application.Contracts.Persistence;

namespace Web.Endpoints;

public static class BlobStorageEndpoints
{
    public static IEndpointRouteBuilder MapBlobStorageEndpoints(this IEndpointRouteBuilder app)
    {
        //TODO can upload any file type by just changing the content type in Windows File Dialog to All
        // can upload inifnite amount of files by just uploading one by one or many at once in the Windows File Dialog
        
        app.MapPost("videos", async (IFormFile file, IBlobService blobService) =>
        {
            // Limit size to 50MB
            if (file.Length > 35 * 1024 * 1024)
                return Results.BadRequest("File size exceeds the limit of 35MB.");
            
            // Check if the file is a video
            var allowedVideoTypes = new[] { "video/mp4", "video/x-msvideo", "video/x-matroska", "video/quicktime" };
            if (!allowedVideoTypes.Contains(file.ContentType))
                return Results.BadRequest("Invalid video file type. Allowed types are: mp4, avi, mkv, mov.");
            
            await using var stream = file.OpenReadStream();
            
            var fileID = await blobService.UploadFileAsync(stream, file.ContentType, "videos");
            
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
            // limit 10MB
            if (file.Length > 10 * 1024 * 1024)
                return Results.BadRequest("File size exceeds the limit of 10MB.");
            
            // Check if the file is an image
            var allowedImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedImageTypes.Contains(file.ContentType))
                return Results.BadRequest("Invalid image file type. Allowed types are: jpeg, png, gif, webp.");
            
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