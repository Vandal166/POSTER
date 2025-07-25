using Application.Contracts.Persistence;
using Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

    public async Task<string> UpdateAvatarAsync(string userID, AvatarDto file, CancellationToken ct = default)
    {
        // Ensure avatars folder exists
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // Generate unique file name
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{userID}{extension}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // Save file
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.Content.CopyToAsync(stream, ct);
        }

        // Return relative path (used for database)
        return Path.Combine("uploads", "avatars", fileName).Replace("\\", "/");
    }
}
