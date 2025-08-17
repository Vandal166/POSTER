namespace Application.DTOs;

public record UserProfileDto(Guid Id, string Username, string AvatarPath, DateTime JoinedAt);