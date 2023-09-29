using System.Diagnostics;
using ASP.NET_HW_20.Models;
using ASP.NET_HW_20.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_HW_20.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IUserHelper _userHelper;

    public HomeController(ILogger<HomeController> logger, IUserHelper userHelper) {
        _logger = logger;
        _userHelper = userHelper;
    }

    public IActionResult Index() {
        if (User.Identity!.IsAuthenticated) {
            _userHelper.IncreaseVisitCountAsync(User);
            _userHelper.RefreshUserAsync(User);
        }

        return View();
    }

    [Authorize(Roles = "Experienced")]
    public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}