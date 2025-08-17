namespace Application.DTOs;

public class ToastMessage
{
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; } = ToastType.Success;
}