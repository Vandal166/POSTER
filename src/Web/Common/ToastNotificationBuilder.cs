using Application.DTOs;
using FluentResults;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Web.Contracts;

namespace Web.Common;

internal sealed class ToastNotificationBuilder : IToastBuilder
{
    private ToastMessage _toastMessage = new ToastMessage();

    public IToastBuilder SetToast<TValue>(Result<TValue> result)
    {
        if (result.IsFailed)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Message));
            _toastMessage = new ToastMessage { Message = errorMessage, Type = ToastType.Error };
        }
        else
        {
            _toastMessage = new ToastMessage { Message = "Success", Type = ToastType.Success };
        }
        return this;
    }
    
    public IToastBuilder SetToast(string messageResult, ToastType type)
    {
        _toastMessage = new ToastMessage { Message = messageResult, Type = type };
        
        return this;
    }

    public IToastBuilder OnSuccess(string message)
    {
        if (_toastMessage.Type == ToastType.Success)
            _toastMessage.Message = message;
        return this;
    }

    public IToastBuilder OnError(string message)
    {
        if (_toastMessage.Type == ToastType.Error)
            _toastMessage.Message = message;
        return this;
    }

    public void Build(ITempDataDictionary tempData)
    {
        tempData.AddToast(_toastMessage);
    }
}