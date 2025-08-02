namespace Application.DTOs;

public record AvatarDto(string FileName, Stream Content);


public record FileResponseDto(Stream stream, string ContentType);