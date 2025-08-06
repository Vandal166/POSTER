using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;
using Application.DTOs;

namespace Web.Common;

public static class TempDataExtensions
{
    private const string ToastKey = "ToastMessage";

    public static void ShowSuccess(this ITempDataDictionary tempData, string message)
        => tempData.AddToast(new ToastMessage { Message = message, Type = ToastType.Success });

    public static void ShowError(this ITempDataDictionary tempData, string message)
        => tempData.AddToast(new ToastMessage { Message = message, Type = ToastType.Error });
    
    
    public static void AddToast(this ITempDataDictionary tempData, ToastMessage toast)
    {
        var toasts = tempData.GetToast() ?? new List<ToastMessage>();
        toasts.Add(toast);
        tempData[ToastKey] = JsonSerializer.Serialize(toasts);
    }
    
    public static List<ToastMessage>? GetToast(this ITempDataDictionary tempData)
    {
        if (tempData.TryGetValue(ToastKey, out var obj) && obj is string json && !string.IsNullOrEmpty(json))
        {
            return JsonSerializer.Deserialize<List<ToastMessage>>(json);
        }
        return null;
    }
}