using Microsoft.AspNetCore.Mvc;

namespace Web.Common.ViewComponents;

public class ToastViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var toast = TempData.GetToast();
        return View(toast);
    }
}
