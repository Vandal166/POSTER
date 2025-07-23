namespace Application.DTOs;

public record TokenResponse(string AccessToken, string? IdToken, string RefreshToken, string UserId);