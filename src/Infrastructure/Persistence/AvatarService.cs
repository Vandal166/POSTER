using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Persistence;

//TODO Attirubte
//<a href="https://www.flaticon.com/free-icons/profile-image" title="profile-image icons">Profile-image icons created by Md Tanvirul Haque - Flaticon</a>
public class AvatarService : IAvatarService
{
    private readonly IWebHostEnvironment _env;

    public AvatarService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UpdateAvatarAsync(Guid userID, AvatarDto file, CancellationToken ct = default)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // generating unique file name based on userID
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{userID}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // deleting existing avatar file if it exists
        if (Directory.GetFiles(uploadsPath, $"{userID}.*").Length > 0)
        {
            var existingFile = Directory.GetFiles(uploadsPath, $"{userID}.*").FirstOrDefault();
            if (existingFile != null)
            {
                try
                {
                    File.Delete(existingFile); // deleting the old one
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting existing avatar file {ex.Message}");
                }
            }
        }
        // saving the file
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.Content.CopyToAsync(stream, ct);
        }

        // returning relative path (used for database)
        return Path.Combine("uploads", "avatars", fileName).Replace("\\", "/");
    }
}
