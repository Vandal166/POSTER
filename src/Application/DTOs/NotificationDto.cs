namespace Application.DTOs;

public record NotificationDto(Guid Id, string Message, DateTime CreatedAt, bool IsRead, string? RedirectUrl);