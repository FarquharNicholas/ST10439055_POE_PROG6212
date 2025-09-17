using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ST10439055_POE_PROG6212.Models;

namespace ST10439055_POE_PROG6212.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();   // Home Page
        public IActionResult Dashboard() => View();
        public IActionResult SubmitClaim() => View();
        public IActionResult ViewClaims() => View();
        public IActionResult UploadDocs() => View();
        public IActionResult AdminReview() => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
