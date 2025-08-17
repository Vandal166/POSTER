using Application.DTOs;

namespace Application.Contracts.Persistence;

// Interface for basic crud operations on videos/images in Azure blob storage
public interface IBlobService
{
    Task<Guid> UploadFileAsync(Stream fileStream, string contentType, string containerName, CancellationToken ct = default);
    
    Task<FileResponseDto> DownloadFileAsync(Guid fileID, string containerName, CancellationToken ct = default);
    
    Task DeleteFileAsync(Guid fileID, string containerName, CancellationToken ct = default);
}