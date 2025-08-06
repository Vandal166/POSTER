using Application.Contracts.Persistence;
using Application.DTOs;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Application.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Guid> UploadFileAsync(Stream fileStream, string contentType, string containerName, CancellationToken ct = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var fileID = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient(fileID.ToString());

        try
        {
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
            {
                ContentType = contentType
            }, cancellationToken: ct);
            
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e.Message);
        }
        return fileID;
    }

    public async Task<FileResponseDto> DownloadFileAsync(Guid fileID, string containerName, CancellationToken ct = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        var blobClient = containerClient.GetBlobClient(fileID.ToString());

        try
        {
            Response<BlobDownloadResult> response = await blobClient.DownloadContentAsync(cancellationToken: ct);
            
            if (response.Value.Content is null)
                throw new FileNotFoundException($"File with ID {fileID} not found in blob storage.");
            
            return new FileResponseDto(response.Value.Content.ToStream(), response.Value.Details.ContentType); // returning the file stream and content type from the blob
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while downloading the file: {e.Message}");
        }
        
        return new FileResponseDto(null, null); // return null if the file does not exist
    }

    public async Task DeleteFileAsync(Guid fileID, string containerName, CancellationToken ct = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        var blobClient = containerClient.GetBlobClient(fileID.ToString());
        
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }
}