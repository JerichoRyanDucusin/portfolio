using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers;

// Shows friendly pages instead of the raw error screens.
public class ErrorController : Controller
{
    // /Error/Status/404 : a page that was not found (or any other status code).
    public IActionResult Status(int id)
    {
        ViewBag.Code = id;
        return View();
    }

    // /Error/Unexpected : something crashed (for example the database was unreachable).
    public IActionResult Unexpected()
    {
        return View();
    }

    // TEMPORARY: delete this action after the test.
    public IActionResult Boom() => throw new Exception("test");
}