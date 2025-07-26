using Application.DTOs;

namespace Infrastructure.Auth;

public static class KeycloakClientFactory
{
    public static object CreateKeycloakUser(RegisterUserDto dto)
    {
        return new 
        {
            username = dto.Email,
            email    = dto.Email,
            enabled  = true,
            attributes = new {
                profileCompleted = new[] { "false" },
                avatarPath = new[] { "uploads/avatars/default.png" }
            },
            credentials = new[] {
                new { type = "password", value = dto.Password, temporary = false }
            }
        };
    }
}