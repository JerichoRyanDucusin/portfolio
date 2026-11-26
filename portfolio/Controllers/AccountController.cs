using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using portfolio.Models;

namespace portfolio.Controllers;

// Sign in and sign out for the editor side.
public class AccountController : Controller
{
    private readonly IConfiguration _config;

    public AccountController(IConfiguration config)
    {
        _config = config;
    }

    // Compares two texts in constant time, so the answer time reveals nothing.
    private static bool Same(string? given, string? expected)
    {
        if (string.IsNullOrEmpty(given) || string.IsNullOrEmpty(expected))
        {
            return false;
        }
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(given));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginForm());
    }

    // POST /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginForm form, string? returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(form);
        }

        // The real username and password come from User Secrets, never from the code.
        bool ok = Same(form.Username, _config["Editor:Username"])
               && Same(form.Password, _config["Editor:Password"]);

        if (!ok)
        {
            ModelState.AddModelError("", "Wrong username or password.");
            return View(form);
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, form.Username) },
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "ManageCourses");
    }

    // POST /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Courses");
    }
}