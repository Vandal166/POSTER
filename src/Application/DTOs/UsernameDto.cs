namespace Application.DTOs;

public record UsernameDto(string Username);

public record UserDto(Guid Id, string Username, string AvatarPath);