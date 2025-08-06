using Application.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Web.Contracts;

/// <summary>
/// Sets the ITempDataDictionary for toast notifications.
/// The TempData is read in the layout page and displayed as a toast notification upon redirect
/// </summary>
public interface IToastBuilder
{
    IToastBuilder SetToast<TValue>(Result<TValue> result);
    IToastBuilder SetToast(string messageResult, ToastType type);
    
    IToastBuilder OnSuccess(string message);
    IToastBuilder OnError(string message);
    
    void Build(ITempDataDictionary tempData);
}